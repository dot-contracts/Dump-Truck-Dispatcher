using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;
using DispatcherWeb.Offices;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public class UserOrganizationUnitCache : IUserOrganizationUnitCache, ISingletonDependency,
        IAsyncEventHandler<EntityDeletedEventData<UserOrganizationUnit>>,
        IAsyncEventHandler<EntityCreatedEventData<UserOrganizationUnit>>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<Office> _officeRepository;

        public IOrganizationUnitCache OrganizationUnitCache { get; }
        public IExtendedAbpSession Session { get; }

        public UserOrganizationUnitCache(
            IOrganizationUnitCache organizationUnitCache,
            IExtendedAbpSession session,
            ICacheManager cacheManager,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<Office> officeRepository
        )
        {
            OrganizationUnitCache = organizationUnitCache;
            Session = session;
            _cacheManager = cacheManager;
            _unitOfWorkManager = unitOfWorkManager;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _officeRepository = officeRepository;
        }

        private ITypedCache<long, List<UserOrganizationUnitCacheItem>> GetCacheInternal()
        {
            return _cacheManager
                .GetCache(UserOrganizationUnitCacheItem.CacheName)
                .AsTyped<long, List<UserOrganizationUnitCacheItem>>();
        }

        public async Task<bool> HasAccessToAllOffices()
        {
            var tenantId = await Session.GetTenantIdOrNullAsync();
            if (Session.UserId.HasValue && tenantId.HasValue)
            {
                return await HasAccessToAllOffices(Session.UserId.Value, tenantId.Value);
            }
            return false;
        }

        public async Task<bool> HasAccessToAllOffices(long userId, int tenantId)
        {
            var allOffices = await OrganizationUnitCache.GetOfficeBasedOrganizationUnitsAsync(tenantId);
            var userOffices = await GetUserOrganizationUnitsAsync(userId);
            return allOffices.All(o => userOffices.Any(uo => uo.OfficeId == o.OfficeId));
        }

        public async Task<List<UserOrganizationUnitCacheItem>> GetUserOrganizationUnitsAsync()
        {
            if (!Session.UserId.HasValue)
            {
                return new List<UserOrganizationUnitCacheItem>();
            }

            return await GetUserOrganizationUnitsAsync(Session.UserId.Value);
        }

        public async Task<List<UserOrganizationUnitCacheItem>> GetUserOrganizationUnitsAsync(long userId)
        {
            return await GetCacheInternal()
                .GetAsync(userId, async k =>
                {
                    return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                    {
                        var userOrganizationUnits = await
                            (from ou in await _userOrganizationUnitRepository.GetQueryAsync()
                             join o in await _officeRepository.GetQueryAsync() on ou.OrganizationUnitId equals o.OrganizationUnitId into offices
                             from office in offices.DefaultIfEmpty()
                             where ou.UserId == userId
                             select new UserOrganizationUnitCacheItem
                             {
                                 UserId = ou.UserId,
                                 OrganizationUnitId = ou.OrganizationUnitId,
                                 OfficeId = office == null ? null : office.Id,
                             }).ToListAsync();

                        return userOrganizationUnits;
                    });
                });
        }

        public async Task HandleEventAsync(EntityDeletedEventData<UserOrganizationUnit> eventData)
        {
            await GetCacheInternal().RemoveAsync(eventData.Entity.UserId);
        }

        public async Task HandleEventAsync(EntityCreatedEventData<UserOrganizationUnit> eventData)
        {
            await GetCacheInternal().RemoveAsync(eventData.Entity.UserId);
        }
    }
}
