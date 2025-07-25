using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.LeaseHaulers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulers.Cache
{
    public class LeaseHaulerTruckListCache : ListCacheBase<ListCacheTenantKey, LeaseHaulerTruckCacheItem, LeaseHaulerTruck>,
        ILeaseHaulerTruckListCache,
        ISingletonDependency
    {
        private readonly IRepository<LeaseHaulerTruck> _leaseHaulerTruckRepository;
        public override string CacheName => ListCacheNames.LeaseHaulerTruck;

        public LeaseHaulerTruckListCache(
            IRepository<LeaseHaulerTruck> leaseHaulerTruckRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _leaseHaulerTruckRepository = leaseHaulerTruckRepository;
        }

        protected override async Task<List<LeaseHaulerTruckCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _leaseHaulerTruckRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new LeaseHaulerTruckCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    LeaseHaulerId = x.LeaseHaulerId,
                    TruckId = x.TruckId,
                    AlwaysShowOnSchedule = x.AlwaysShowOnSchedule,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(LeaseHaulerTruck entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
