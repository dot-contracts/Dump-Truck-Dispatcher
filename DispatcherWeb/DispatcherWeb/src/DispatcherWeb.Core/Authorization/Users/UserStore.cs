using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Organizations;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Caching;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users
{
    /// <summary>
    /// Used to perform database operations for <see cref="UserManager"/>.
    /// </summary>
    public class UserStore : AbpUserStore<Role, User>
    {
        private readonly EntityListCacheCollection _entityListCaches;
        private readonly IRepository<UserLogin, long> _userLoginRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<UserClaim, long> _userClaimRepository;

        public UserStore(
            EntityListCacheCollection entityListCaches,
            IRepository<User, long> userRepository,
            IRepository<UserLogin, long> userLoginRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<Role> roleRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<UserClaim, long> userClaimRepository,
            IRepository<UserPermissionSetting, long> userPermissionSettingRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository)
            : base(
                unitOfWorkManager,
                userRepository,
                roleRepository,
                userRoleRepository,
                userLoginRepository,
                userClaimRepository,
                userPermissionSettingRepository,
                userOrganizationUnitRepository,
                organizationUnitRoleRepository)
        {
            _entityListCaches = entityListCaches;
            _userLoginRepository = userLoginRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _userClaimRepository = userClaimRepository;
        }

        protected async Task<ListCacheTenantKey> GetCacheKeyAsync()
        {
            return new ListCacheTenantKey(await GetTenantIdAsync() ?? 0);
        }

        protected async Task<int?> GetTenantIdAsync()
        {
            if (_unitOfWorkManager.Current != null)
            {
                return _unitOfWorkManager.Current.GetTenantId();
            }

            return await AbpSession.GetTenantIdOrNullAsync();
        }

        /// <inheritdoc />
        public override async Task<User> FindByIdAsync(string userId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var users = await _entityListCaches.User.GetList(await GetCacheKeyAsync());
            var user = users.Find(userId.To<long>())?.Clone();

            if (user == null && UserStoreConfiguration?.FallbackToDatabaseOnCacheMisses == true)
            {
                user = await base.FindByIdAsync(userId, cancellationToken);
            }

            return user;
        }

        /// <inheritdoc />
        public override async Task<User> FindByNameAsync([NotNull] string normalizedUserName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Check.NotNull(normalizedUserName, nameof(normalizedUserName));

            var users = await _entityListCaches.User.GetList(await GetCacheKeyAsync());
            return users.Items.FirstOrDefault(u => u.NormalizedUserName?.ToUpperInvariant() == normalizedUserName.ToUpperInvariant())?.Clone();
        }

        #region Roles

        /// <inheritdoc />
        public override async Task AddToRoleAsync([NotNull] User user, [NotNull] string normalizedRoleName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(user, nameof(user));
            Check.NotNull(normalizedRoleName, nameof(normalizedRoleName));

            if (await IsInRoleAsync(user, normalizedRoleName, cancellationToken))
            {
                return;
            }

            var roles = await _entityListCaches.Role.GetList(await GetCacheKeyAsync());
            var role = roles.Items.FirstOrDefault(r => r.NormalizedName?.ToUpperInvariant() == normalizedRoleName.ToUpperInvariant()); //no need to clone as long as we're only using the id

            if (role == null && UserStoreConfiguration?.FallbackToDatabaseOnCacheMisses == true)
            {
                role = await _roleRepository.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName);
            }

            if (role == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Role {0} does not exist!", normalizedRoleName));
            }

            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _userRoleRepository.InsertAsync(new UserRole(user.TenantId, user.Id, role.Id));
            });
        }

        /// <inheritdoc />
        public override async Task RemoveFromRoleAsync(
            [NotNull] User user,
            [NotNull] string normalizedRoleName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(user, nameof(user));

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException(nameof(normalizedRoleName) + " can not be null or whitespace");
            }

            if (!await IsInRoleAsync(user, normalizedRoleName, cancellationToken))
            {
                return;
            }

            var roles = await _entityListCaches.Role.GetList(await GetCacheKeyAsync());
            var role = roles.Items.FirstOrDefault(r => r.NormalizedName?.ToUpperInvariant() == normalizedRoleName.ToUpperInvariant()); //no need to clone as long as we're only using the id
            if (role == null)
            {
                return;
            }

            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                await _userRoleRepository.DeleteAsync(r => r.RoleId == role.Id && r.UserId == user.Id);
            });
        }

        /// <inheritdoc />
        public override async Task<IList<string>> GetRolesAsync(
            [NotNull] User user,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Check.NotNull(user, nameof(user));

            var key = await GetCacheKeyAsync();
            var userRoles = await _entityListCaches.UserRole.GetList(key);
            var roles = await _entityListCaches.Role.GetList(key);

            var userRoleNames =
                from userRole in userRoles.Items
                join role in roles.Items on userRole.RoleId equals role.Id
                where userRole.UserId == user.Id
                select role.Name;

            if (UserStoreConfiguration?.UseOrganizationUnitRoles != true)
            {
                return userRoleNames.ToList();
            }

            var userOrganizationUnits = await _entityListCaches.UserOrganizationUnit.GetList(key);
            var organizationUnitRoles = await _entityListCaches.OrganizationUnitRole.GetList(key);
            var userOrganizationUnitRoleNames =
                from userOu in userOrganizationUnits.Items
                join roleOu in organizationUnitRoles.Items on userOu.OrganizationUnitId equals roleOu.OrganizationUnitId
                join userOuRoles in roles.Items on roleOu.RoleId equals userOuRoles.Id
                where userOu.UserId == user.Id
                select userOuRoles.Name;

            return userRoleNames.Union(userOrganizationUnitRoleNames).ToList();
        }

        /// <inheritdoc />
        public override async Task<bool> IsInRoleAsync(
            [NotNull] User user,
            [NotNull] string normalizedRoleName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(user, nameof(user));

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException(nameof(normalizedRoleName) + " can not be null or whitespace");
            }

            return (await GetRolesAsync(user, cancellationToken)).Any(r => r.ToUpperInvariant() == normalizedRoleName);
        }

        #endregion

        #region UserClaims

        /// <inheritdoc />
        public override async Task<IList<Claim>> GetClaimsAsync(
            [NotNull] User user,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredUserClaims != true)
            {
                return new List<Claim>();
            }
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(user, nameof(user));

                return (await (await _userClaimRepository.GetQueryAsync())
                        .Where(x => x.UserId == user.Id && x.TenantId == user.TenantId)
                        .ToListAsync(cancellationToken))
                    .Select(c => new Claim(c.ClaimType, c.ClaimValue))
                    .ToList();
            });
        }

        /// <inheritdoc />
        public override async Task AddClaimsAsync(
            [NotNull] User user,
            [NotNull] IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredUserClaims != true)
            {
                return;
            }
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(user, nameof(user));
                Check.NotNull(claims, nameof(claims));

                await _userClaimRepository.InsertRangeAsync(claims.Select(claim => new UserClaim(user, claim)));
            });
        }

        /// <inheritdoc />
        public override async Task ReplaceClaimAsync(
            [NotNull] User user,
            [NotNull] Claim claim,
            [NotNull] Claim newClaim,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredUserClaims != true)
            {
                return;
            }
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(user, nameof(user));
                Check.NotNull(claim, nameof(claim));
                Check.NotNull(newClaim, nameof(newClaim));

                var userClaims = await (await _userClaimRepository.GetQueryAsync())
                    .Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type && uc.UserId == user.Id)
                    .ToListAsync(cancellationToken);

                foreach (var userClaim in userClaims)
                {
                    userClaim.ClaimType = newClaim.Type;
                    userClaim.ClaimValue = newClaim.Value;
                }
            });
        }

        /// <inheritdoc />
        public override async Task RemoveClaimsAsync(
            [NotNull] User user,
            [NotNull] IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredUserClaims != true)
            {
                return;
            }
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(user, nameof(user));
                Check.NotNull(claims, nameof(claims));

                var dbClaims = await (await _userClaimRepository.GetQueryAsync())
                    .Where(x => x.UserId == user.Id)
                    .ToListAsync(cancellationToken);

                foreach (var claim in claims)
                {
                    var claimsToDelete = dbClaims.Where(c => c.ClaimValue == claim.Value && c.ClaimType == claim.Type).ToList();
                    if (!claimsToDelete.Any())
                    {
                        continue;
                    }
                    await _userClaimRepository.DeleteRangeAsync(claimsToDelete);
                }
            });
        }

        #endregion

        #region UserLogins

        /// <inheritdoc />
        public override async Task AddLoginAsync(
            [NotNull] User user,
            [NotNull] UserLoginInfo login,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(user, nameof(user));
                Check.NotNull(login, nameof(login));

                await _userLoginRepository.InsertAsync(new UserLogin(user.TenantId, user.Id, login.LoginProvider, login.ProviderKey));
            });
        }

        /// <inheritdoc />
        public override async Task RemoveLoginAsync(
            [NotNull] User user,
            [NotNull] string loginProvider,
            [NotNull] string providerKey,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(user, nameof(user));
                Check.NotNull(loginProvider, nameof(loginProvider));
                Check.NotNull(providerKey, nameof(providerKey));

                await _userLoginRepository.DeleteAsync(userLogin =>
                    userLogin.LoginProvider == loginProvider
                    && userLogin.ProviderKey == providerKey
                    && userLogin.UserId == user.Id
                );
            });
        }

        /// <inheritdoc />
        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(
            [NotNull] User user,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var userLogins = await _entityListCaches.UserLogin.GetList(await GetCacheKeyAsync());
            return userLogins.Items
                .Where(x => x.UserId == user.Id)
                .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.LoginProvider))
                .ToList();
        }

        /// <inheritdoc />
        public override async Task<User> FindByLoginAsync(
            [NotNull] string loginProvider,
            [NotNull] string providerKey,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = await GetCacheKeyAsync();
            var userLogins = await _entityListCaches.UserLogin.GetList(key);
            var users = await _entityListCaches.User.GetList(key);

            var query =
                from userLogin in userLogins.Items
                join user in users.Items on userLogin.UserId equals user.Id
                where userLogin.LoginProvider == loginProvider
                    && userLogin.ProviderKey == providerKey
                select user;

            return query.FirstOrDefault()?.Clone();
        }

        #endregion

        /// <inheritdoc />
        public override async Task<User> FindByEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var users = await _entityListCaches.User.GetList(await GetCacheKeyAsync());
            return users.Items.FirstOrDefault(u => u.NormalizedEmailAddress?.ToUpperInvariant() == normalizedEmail?.ToUpperInvariant())?.Clone();
        }

        /// <inheritdoc />
        public override async Task<IList<User>> GetUsersForClaimAsync(
            [NotNull] Claim claim,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredUserClaims != true)
            {
                return new List<User>();
            }
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(claim, nameof(claim));

                var tenantId = await AbpSession.GetTenantIdOrNullAsync();
                var query = from userclaims in await _userClaimRepository.GetQueryAsync()
                            join user in await UserRepository.GetQueryAsync() on userclaims.UserId equals user.Id
                            where userclaims.ClaimValue == claim.Value
                                  && userclaims.ClaimType == claim.Type
                                  && userclaims.TenantId == tenantId
                            select user;

                return await AsyncQueryableExecuter.ToListAsync(query);
            });
        }

        /// <inheritdoc />
        public override async Task<IList<User>> GetUsersInRoleAsync(
            [NotNull] string normalizedRoleName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var key = await GetCacheKeyAsync();

            var roles = await _entityListCaches.Role.GetList(key);
            var role = roles.Items.FirstOrDefault(r => r.NormalizedName?.ToUpperInvariant() == normalizedRoleName.ToUpperInvariant()); //no need to clone as long as we're only using the id
            if (role == null)
            {
                return new List<User>();
            }

            var userRoles = await _entityListCaches.UserRole.GetList(key);
            var users = await _entityListCaches.User.GetList(key);

            var query =
                from userRole in userRoles.Items
                join user in users.Items on userRole.UserId equals user.Id
                where userRole.RoleId.Equals(role.Id)
                select user?.Clone();

            return query.ToList();
        }

        #region UserTokens

        /// <inheritdoc />
        public override async Task SetTokenAsync(
            [NotNull] User user,
            string loginProvider,
            string name,
            string value,
            CancellationToken cancellationToken)
        {
            if (UserStoreConfiguration?.UseDbStoredUserTokens != true)
            {
                return;
            }

            await Task.CompletedTask;
            //await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            //{
            //    cancellationToken.ThrowIfCancellationRequested();

            //    Check.NotNull(user, nameof(user));

            //    await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, cancellationToken);

            //    var token = user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name);
            //    if (token == null)
            //    {
            //        user.Tokens.Add(new UserToken(user, loginProvider, name, value));
            //    }
            //    else
            //    {
            //        token.Value = value;
            //    }
            //});
        }

        /// <inheritdoc />
        public override async Task RemoveTokenAsync(
            User user,
            string loginProvider,
            string name,
            CancellationToken cancellationToken)
        {
            if (UserStoreConfiguration?.UseDbStoredUserTokens != true)
            {
                return;
            }

            await Task.CompletedTask;
            //await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            //{
            //    cancellationToken.ThrowIfCancellationRequested();
            //    Check.NotNull(user, nameof(user));
            //    await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, cancellationToken);
            //    user.Tokens.RemoveAll(t => t.LoginProvider == loginProvider && t.Name == name);
            //});
        }

        /// <inheritdoc />
        public override async Task<string> GetTokenAsync(User user, string loginProvider, string name,
            CancellationToken cancellationToken)
        {
            if (UserStoreConfiguration?.UseDbStoredUserTokens != true)
            {
                return null;
            }

            await Task.CompletedTask;

            return null;
            //return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            //{
            //    cancellationToken.ThrowIfCancellationRequested();
            //    Check.NotNull(user, nameof(user));
            //    await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, cancellationToken);
            //    return user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name)?.Value;
            //});
        }

        #endregion

        /// <inheritdoc />
        public override async Task<User> FindByNameOrEmailAsync(string userNameOrEmailAddress)
        {
            var normalizedUserNameOrEmailAddress = NormalizeKey(userNameOrEmailAddress);

            var users = await _entityListCaches.User.GetList(await GetCacheKeyAsync());
            return users.Items.FirstOrDefault(
                user => user.NormalizedUserName?.ToUpperInvariant() == normalizedUserNameOrEmailAddress
                        || user.NormalizedEmailAddress?.ToUpperInvariant() == normalizedUserNameOrEmailAddress
            )?.Clone();
        }

        /// <inheritdoc />
        public override async Task<User> FindByNameOrEmailAsync(int? tenantId, string userNameOrEmailAddress)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    return await FindByNameOrEmailAsync(userNameOrEmailAddress);
                }
            });
        }

        public override async Task<List<User>> FindAllAsync(UserLoginInfo login)
        {
            var key = await GetCacheKeyAsync();
            var userLogins = await _entityListCaches.UserLogin.GetList(key);
            var users = await _entityListCaches.User.GetList(key);

            var query =
                from userLogin in userLogins.Items
                join user in users.Items on userLogin.UserId equals user.Id
                where userLogin.LoginProvider == login.LoginProvider && userLogin.ProviderKey == login.ProviderKey
                select user?.Clone();

            return query.ToList();
        }

        public override async Task<User> FindAsync(int? tenantId, UserLoginInfo login)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    return await FindByLoginAsync(login.LoginProvider, login.ProviderKey);
                }
            });
        }


        public override async Task AddTokenValidityKeyAsync(
            [NotNull] User user,
            string tokenValidityKey,
            DateTime expireDate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredUserTokens != true)
            {
                return;
            }

            await Task.CompletedTask;
            //await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            //{
            //    cancellationToken.ThrowIfCancellationRequested();
            //    Check.NotNull(user, nameof(user));
            //    await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, cancellationToken);
            //    user.Tokens.Add(new UserToken(user, TokenValidityKeyProvider, tokenValidityKey, null, expireDate));
            //});
        }

        public override async Task<bool> IsTokenValidityKeyValidAsync(
            [NotNull] User user,
            string tokenValidityKey,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredUserTokens != true)
            {
                return false;
            }

            await Task.CompletedTask;
            return false;
            //return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            //{
            //    cancellationToken.ThrowIfCancellationRequested();
            //    Check.NotNull(user, nameof(user));
            //    await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, cancellationToken);
            //    return user.Tokens.Any(t => t.LoginProvider == TokenValidityKeyProvider
            //                                && t.Name == tokenValidityKey
            //                                && t.ExpireDate > DateTime.UtcNow);
            //});
        }

        public override async Task RemoveTokenValidityKeyAsync(
            [NotNull] User user,
            string tokenValidityKey,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredUserTokens != true)
            {
                return;
            }

            await Task.CompletedTask;
            //await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            //{
            //    cancellationToken.ThrowIfCancellationRequested();
            //    Check.NotNull(user, nameof(user));
            //    await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, cancellationToken);
            //    user.Tokens.RemoveAll(t => t.LoginProvider == TokenValidityKeyProvider && t.Name == tokenValidityKey);
            //});
        }
    }
}
