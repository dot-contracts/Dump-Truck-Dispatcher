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
    public class LeaseHaulerUserListCache : ListCacheBase<ListCacheTenantKey, LeaseHaulerUserCacheItem, LeaseHaulerUser>,
        ILeaseHaulerUserListCache,
        ISingletonDependency
    {
        private readonly IRepository<LeaseHaulerUser> _leaseHaulerUserRepository;
        public override string CacheName => ListCacheNames.LeaseHaulerUser;

        public LeaseHaulerUserListCache(
            IRepository<LeaseHaulerUser> leaseHaulerUserRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _leaseHaulerUserRepository = leaseHaulerUserRepository;
        }

        protected override async Task<List<LeaseHaulerUserCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _leaseHaulerUserRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new LeaseHaulerUserCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    LeaseHaulerId = x.LeaseHaulerId,
                    UserId = x.UserId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(LeaseHaulerUser entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
