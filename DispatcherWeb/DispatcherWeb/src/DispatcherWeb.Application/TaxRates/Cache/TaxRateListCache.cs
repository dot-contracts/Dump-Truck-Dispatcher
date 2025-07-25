using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.TaxRates.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.TaxRates.Cache
{
    public class TaxRateListCache : ListCacheBase<ListCacheTenantKey, TaxRateCacheItem, TaxRate>,
        ITaxRateListCache,
        ISingletonDependency
    {
        private readonly IRepository<TaxRate> _repository;
        public override string CacheName => ListCacheNames.TaxRate;

        public TaxRateListCache(
            IRepository<TaxRate> repository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _repository = repository;
        }

        protected override async Task<List<TaxRateCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _repository.GetQueryAsync(), afterDateTime)
                .Select(x => new TaxRateCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    Name = x.Name,
                    Rate = x.Rate,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(TaxRate entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
