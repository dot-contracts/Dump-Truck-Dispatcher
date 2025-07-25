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
    public class LeaseHaulerListCache : ListCacheBase<ListCacheTenantKey, LeaseHaulerCacheItem, LeaseHauler>,
        ILeaseHaulerListCache,
        ISingletonDependency
    {
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        public override string CacheName => ListCacheNames.LeaseHauler;

        public LeaseHaulerListCache(
            IRepository<LeaseHauler> leaseHaulerRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _leaseHaulerRepository = leaseHaulerRepository;
        }

        protected override async Task<List<LeaseHaulerCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _leaseHaulerRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new LeaseHaulerCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    Name = x.Name,
                    IsActive = x.IsActive,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(LeaseHauler entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
