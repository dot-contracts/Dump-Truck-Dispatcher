using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.VehicleCategories.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.VehicleCategories.Cache
{
    public class VehicleCategoryListCache : ListCacheBase<ListCacheEmptyKey, VehicleCategoryCacheItem, VehicleCategory>,
        IVehicleCategoryListCache,
        ISingletonDependency
    {
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        public override string CacheName => ListCacheNames.VehicleCategory;

        public VehicleCategoryListCache(
            IRepository<VehicleCategory> vehicleCategoryRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _vehicleCategoryRepository = vehicleCategoryRepository;
        }

        protected override async Task<List<VehicleCategoryCacheItem>> GetAllItemsFromDb(ListCacheEmptyKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _vehicleCategoryRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new VehicleCategoryCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    Name = x.Name,
                    AssetType = x.AssetType,
                    IsPowered = x.IsPowered,
                    SortOrder = x.SortOrder,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheEmptyKey> GetKeyFromEntity(VehicleCategory entity)
        {
            return Task.FromResult(ListCacheEmptyKey.Instance);
        }
    }
}
