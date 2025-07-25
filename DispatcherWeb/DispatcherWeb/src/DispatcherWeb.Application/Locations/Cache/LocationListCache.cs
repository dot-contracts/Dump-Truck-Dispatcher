using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Locations.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Locations.Cache
{
    public class LocationListCache : ListCacheBase<ListCacheTenantKey, LocationCacheItem, Location>,
        ILocationListCache,
        ISingletonDependency
    {
        private readonly IRepository<Location> _locationRepository;
        public override string CacheName => ListCacheNames.Location;

        public LocationListCache(
            IRepository<Location> locationRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _locationRepository = locationRepository;
        }

        protected override async Task<List<LocationCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _locationRepository.GetQueryAsync(), afterDateTime)
                .Select(l => new LocationCacheItem
                {
                    Id = l.Id,
                    IsDeleted = l.IsDeleted,
                    DeletionTime = l.DeletionTime,
                    CreationTime = l.CreationTime,
                    LastModificationTime = l.LastModificationTime,
                    DisplayName = l.DisplayName,
                    IsActive = l.IsActive,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(Location entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
