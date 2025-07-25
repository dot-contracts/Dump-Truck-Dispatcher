using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulers.Cache
{
    public class AvailableLeaseHaulerTruckListCache : ListCacheBase<ListCacheDateKey, AvailableLeaseHaulerTruckCacheItem, AvailableLeaseHaulerTruck>,
        IAvailableLeaseHaulerTruckListCache,
        ISingletonDependency
    {
        private readonly IRepository<AvailableLeaseHaulerTruck> _repository;
        public override string CacheName => ListCacheNames.AvailableLeaseHaulerTruck;

        public AvailableLeaseHaulerTruckListCache(
            IRepository<AvailableLeaseHaulerTruck> repository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _repository = repository;
        }

        protected override async Task<List<AvailableLeaseHaulerTruckCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _repository.GetQueryAsync(), afterDateTime)
                .Where(x => x.TenantId == key.TenantId
                    && x.Date == key.Date
                    && (x.Shift == key.Shift || key.Shift == null))
                .Select(x => new AvailableLeaseHaulerTruckCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    LeaseHaulerId = x.LeaseHaulerId,
                    TruckId = x.TruckId,
                    DriverId = x.DriverId,
                    OfficeId = x.OfficeId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheDateKey> GetKeyFromEntity(AvailableLeaseHaulerTruck entity)
        {
            return Task.FromResult(new ListCacheDateKey(entity.TenantId, entity.Date, entity.Shift));
        }
    }
}
