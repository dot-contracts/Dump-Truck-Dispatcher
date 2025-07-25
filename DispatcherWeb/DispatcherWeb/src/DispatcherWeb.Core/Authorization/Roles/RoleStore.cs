using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization.Roles;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Caching;
using DispatcherWeb.Runtime.Session;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Roles
{
    public class RoleStore : AbpRoleStore<Role, User>
    {
        public IExtendedAbpSession Session { get; }

        private readonly EntityListCacheCollection _entityListCaches;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<RoleClaim, long> _roleClaimRepository;

        public RoleStore(
            EntityListCacheCollection entityListCaches,
            IUnitOfWorkManager unitOfWorkManager,
            IExtendedAbpSession session,
            IRepository<RoleClaim, long> roleClaimRepository,
            IRepository<Role> roleRepository,
            IRepository<RolePermissionSetting, long> rolePermissionSettingRepository)
            : base(
                unitOfWorkManager,
                roleRepository,
                rolePermissionSettingRepository)
        {
            Session = session;
            _entityListCaches = entityListCaches;
            _unitOfWorkManager = unitOfWorkManager;
            _roleClaimRepository = roleClaimRepository;
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

            return await Session.GetTenantIdOrNullAsync();
        }

        /// <inheritdoc />
        public override async Task<Role> FindByIdAsync(string id,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var roles = await _entityListCaches.Role.GetList(await GetCacheKeyAsync());
            var role = roles.Find(id.To<int>())?.Clone();
            if (role == null && UserStoreConfiguration?.FallbackToDatabaseOnCacheMisses == true)
            {
                return await base.FindByIdAsync(id, cancellationToken);
            }
            return role;
        }

        /// <inheritdoc />
        public override async Task<Role> FindByNameAsync(
            [NotNull] string normalizedName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Check.NotNull(normalizedName, nameof(normalizedName));

            var roles = await _entityListCaches.Role.GetList(await GetCacheKeyAsync());
            var role = roles.Items.FirstOrDefault(r => r.NormalizedName?.ToUpperInvariant() == normalizedName?.ToUpperInvariant())?.Clone();
            return role;
        }

        /// <inheritdoc />
        public override async Task<IList<Claim>> GetClaimsAsync(
            [NotNull] Role role,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredRoleClaims != true)
            {
                return [];
            }
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(role, nameof(role));

                var roleClaims = (await (await _roleClaimRepository.GetQueryAsync())
                        .AsNoTracking()
                        .Where(c => c.RoleId == role.Id)
                        .ToListAsync(cancellationToken))
                    .Select(c => new Claim(c.ClaimType, c.ClaimValue))
                    .ToList();

                return roleClaims;
            });
        }

        /// <inheritdoc />
        public override async Task AddClaimAsync(
            [NotNull] Role role,
            [NotNull] Claim claim,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredRoleClaims != true)
            {
                return;
            }
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Check.NotNull(role, nameof(role));
                Check.NotNull(claim, nameof(claim));

                await _roleClaimRepository.InsertAsync(new RoleClaim(role, claim));
            });
        }

        /// <inheritdoc />
        public override async Task RemoveClaimAsync(
            [NotNull] Role role,
            [NotNull] Claim claim,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserStoreConfiguration?.UseDbStoredRoleClaims != true)
            {
                return;
            }
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                Check.NotNull(role, nameof(role));
                Check.NotNull(claim, nameof(claim));

                await _roleClaimRepository.DeleteAsync(c => c.RoleId == role.Id && c.ClaimValue == claim.Value && c.ClaimType == claim.Type);
            });
        }

        public override async Task<Role> FindByDisplayNameAsync(string displayName)
        {
            var roles = await _entityListCaches.Role.GetList(await GetCacheKeyAsync());
            return roles.Items.FirstOrDefault(r => r.DisplayName?.ToUpperInvariant() == displayName?.ToUpperInvariant())?.Clone();
        }

    }
}
