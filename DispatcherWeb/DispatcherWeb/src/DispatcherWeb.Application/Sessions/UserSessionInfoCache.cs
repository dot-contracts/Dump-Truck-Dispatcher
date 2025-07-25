using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Sessions.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Sessions
{
    public class UserSessionInfoCache : IUserSessionInfoCache, ITransientDependency,
        IAsyncEventHandler<EntityUpdatedEventData<User>>
    {
        public const string UserSessionInfoCacheName = "UserSessionInfo-Cache";

        private readonly ICacheManager _cacheManager;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public UserSessionInfoCache(
            ICacheManager cacheManager,
            UserManager userManager,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _cacheManager = cacheManager;
            _userManager = userManager;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<UserLoginInfoDto> GetUserSessionInfoFromCacheOrSource(long userId, bool disableTenantFilter = false)
        {
            var cache = GetUserSessionInfoCache();
            var cacheItem = await cache.GetOrDefaultAsync(userId.ToString());

            if (cacheItem == null)
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    IDisposable tenantFilter = null;
                    try
                    {
                        if (disableTenantFilter)
                        {
                            tenantFilter = _unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant);
                        }

                        cacheItem = await (await _userManager.GetQueryAsync())
                            .Where(u => u.Id == userId)
                            .Select(u => new UserLoginInfoDto
                            {
                                Id = u.Id,
                                Name = u.Name,
                                Surname = u.Surname,
                                UserName = u.UserName,
                                EmailAddress = u.EmailAddress,
                                ProfilePictureId = u.ProfilePictureId.ToString(),
                            })
                            .FirstOrDefaultAsync();

                        if (cacheItem == null)
                        {
                            throw new Exception("User not found!");
                        }

                        await cache.SetAsync(userId.ToString(), cacheItem);
                    }
                    finally
                    {
                        tenantFilter?.Dispose();
                    }
                }, new UnitOfWorkOptions { IsTransactional = false });
            }

            return cacheItem;
        }

        public async Task InvalidateCache()
        {
            await GetUserSessionInfoCache().ClearAsync();
        }

        public async Task InvalidateCache(long userId)
        {
            await GetUserSessionInfoCache().RemoveAsync(userId.ToString());
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<User> eventData)
        {
            await InvalidateCache(eventData.Entity.Id);
        }

        private ITypedCache<string, UserLoginInfoDto> GetUserSessionInfoCache()
        {
            return _cacheManager
                .GetCache(UserSessionInfoCacheName)
                .AsTyped<string, UserLoginInfoDto>();
        }
    }
}
