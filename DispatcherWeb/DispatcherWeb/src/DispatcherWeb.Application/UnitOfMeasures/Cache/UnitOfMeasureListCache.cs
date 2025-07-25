using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.UnitOfMeasures.Dto;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.UnitOfMeasures.Cache
{
    public class UnitOfMeasureListCache : ListCacheBase<ListCacheTenantKey, UnitOfMeasureCacheItem, UnitOfMeasure>,
        IUnitOfMeasureListCache,
        ISingletonDependency
    {
        private readonly IRepository<UnitOfMeasure> _unitOfMeasureRepository;
        public override string CacheName => ListCacheNames.UnitOfMeasure;

        public UnitOfMeasureListCache(
            IRepository<UnitOfMeasure> unitOfMeasureRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
        }

        protected override async Task<List<UnitOfMeasureCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _unitOfMeasureRepository.GetQueryAsync(), afterDateTime)
                .Select(uom => new UnitOfMeasureCacheItem
                {
                    Id = uom.Id,
                    IsDeleted = uom.IsDeleted,
                    DeletionTime = uom.DeletionTime,
                    CreationTime = uom.CreationTime,
                    LastModificationTime = uom.LastModificationTime,
                    Name = uom.Name,
                    UomBaseId = (UnitOfMeasureBaseEnum?)uom.UnitOfMeasureBaseId,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(UnitOfMeasure entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
