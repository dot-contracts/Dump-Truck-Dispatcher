using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Offices.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Offices.Cache
{
    public class OfficeListCache : ListCacheBase<ListCacheTenantKey, OfficeCacheItem, Office>,
        IOfficeListCache,
        ISingletonDependency
    {
        private readonly IRepository<Office> _repository;
        public override string CacheName => ListCacheNames.Office;

        public OfficeListCache(
            IRepository<Office> repository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _repository = repository;
        }

        protected override async Task<List<OfficeCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _repository.GetQueryAsync(), afterDateTime)
                .Select(x => new OfficeCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    Name = x.Name,
                    OrganizationUnitId = x.OrganizationUnitId,
                    TruckColor = x.TruckColor,
                    CopyDeliverToLoadAtChargeTo = x.CopyDeliverToLoadAtChargeTo,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(Office entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
