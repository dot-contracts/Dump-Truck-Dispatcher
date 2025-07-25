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
    public class LeaseHaulerDriverListCache : ListCacheBase<ListCacheTenantKey, LeaseHaulerDriverCacheItem, LeaseHaulerDriver>,
        ILeaseHaulerDriverListCache,
        ISingletonDependency
    {
        private readonly IRepository<LeaseHaulerDriver> _leaseHaulerDriverRepository;
        public override string CacheName => ListCacheNames.LeaseHaulerDriver;

        public LeaseHaulerDriverListCache(
            IRepository<LeaseHaulerDriver> leaseHaulerDriverRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _leaseHaulerDriverRepository = leaseHaulerDriverRepository;
        }

        protected override async Task<List<LeaseHaulerDriverCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _leaseHaulerDriverRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new LeaseHaulerDriverCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    LeaseHaulerId = x.LeaseHaulerId,
                    DriverId = x.DriverId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(LeaseHaulerDriver entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
