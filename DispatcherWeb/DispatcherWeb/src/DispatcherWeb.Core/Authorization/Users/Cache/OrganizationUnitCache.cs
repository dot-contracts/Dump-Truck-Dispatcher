using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Organizations;
using Abp.Runtime.Caching;
using DispatcherWeb.Offices;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Authorization.Users.Cache
{
    public class OrganizationUnitCache : IOrganizationUnitCache, ISingletonDependency,
        IAsyncEventHandler<EntityDeletedEventData<OrganizationUnit>>,
        IAsyncEventHandler<EntityChangedEventData<OrganizationUnit>>,
        IAsyncEventHandler<EntityCreatedEventData<OrganizationUnit>>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<Office> _officeRepository;

        public IExtendedAbpSession Session { get; }

        public OrganizationUnitCache(
            IExtendedAbpSession session,
            ICacheManager cacheManager,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<Office> officeRepository
        )
        {
            Session = session;
            _cacheManager = cacheManager;
            _unitOfWorkManager = unitOfWorkManager;
            _organizationUnitRepository = organizationUnitRepository;
            _officeRepository = officeRepository;
        }

        private ITypedCache<int?, List<OrganizationUnitCacheItem>> GetCacheInternal()
        {
            return _cacheManager
                .GetCache(OrganizationUnitCacheItem.CacheName)
                .AsTyped<int?, List<OrganizationUnitCacheItem>>();
        }

        public async Task<List<OrganizationUnitCacheItem>> GetAllOrganizationUnitsAsync()
        {
            return await GetAllOrganizationUnitsAsync(await Session.GetTenantIdOrNullAsync());
        }

        public async Task<List<OrganizationUnitCacheItem>> GetAllOrganizationUnitsAsync(int? tenantId)
        {
            return await GetCacheInternal()
                .GetAsync(tenantId, async k =>
                {
                    return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                    {
                        var tenantOrganizationUnits = await
                            (from ou in await _organizationUnitRepository.GetQueryAsync()
                             join o in await _officeRepository.GetQueryAsync() on ou.Id equals o.OrganizationUnitId into offices
                             from office in offices.DefaultIfEmpty()
                             where ou.TenantId == tenantId
                             select new OrganizationUnitCacheItem
                             {
                                 Id = ou.Id,
                                 Name = ou.DisplayName,
                                 OfficeId = office == null ? null : office.Id,
                             }).ToListAsync();

                        return tenantOrganizationUnits;
                    });
                });
        }

        public async Task<List<OrganizationUnitCacheItem>> GetOfficeBasedOrganizationUnitsAsync()
        {
            return await GetOfficeBasedOrganizationUnitsAsync(await Session.GetTenantIdOrNullAsync());
        }

        public async Task<List<OrganizationUnitCacheItem>> GetOfficeBasedOrganizationUnitsAsync(int? tenantId)
        {
            var organizationUnits = await GetAllOrganizationUnitsAsync(tenantId);

            return organizationUnits.Where(x => x.OfficeId.HasValue).ToList();
        }

        public async Task HandleEventAsync(EntityDeletedEventData<OrganizationUnit> eventData)
        {
            await GetCacheInternal().RemoveAsync(eventData.Entity.TenantId);
        }

        public async Task HandleEventAsync(EntityChangedEventData<OrganizationUnit> eventData)
        {
            await GetCacheInternal().RemoveAsync(eventData.Entity.TenantId);
        }

        public async Task HandleEventAsync(EntityCreatedEventData<OrganizationUnit> eventData)
        {
            await GetCacheInternal().RemoveAsync(eventData.Entity.TenantId);
        }
    }
}
