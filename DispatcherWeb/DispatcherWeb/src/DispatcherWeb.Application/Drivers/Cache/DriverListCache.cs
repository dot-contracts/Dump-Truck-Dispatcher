using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Drivers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Drivers.Cache
{
    public class DriverListCache : ListCacheBase<ListCacheTenantKey, DriverCacheItem, Driver>,
        IDriverListCache,
        ISingletonDependency
    {
        private readonly IRepository<Driver> _driverRepository;
        public override string CacheName => ListCacheNames.Driver;

        public DriverListCache(
            IRepository<Driver> driverRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _driverRepository = driverRepository;
        }

        protected override async Task<List<DriverCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _driverRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new DriverCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    DateOfHire = x.DateOfHire,
                    IsExternal = x.IsExternal,
                    IsInactive = x.IsInactive,
                    UserId = x.UserId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(Driver entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
