using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.FuelSurchargeCalculations.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.FuelSurchargeCalculations.Cache
{
    public class FuelSurchargeCalculationListCache : ListCacheBase<ListCacheTenantKey, FuelSurchargeCalculationCacheItem, FuelSurchargeCalculation>,
        IFuelSurchargeCalculationListCache,
        ISingletonDependency
    {
        private readonly IRepository<FuelSurchargeCalculation> _repository;
        public override string CacheName => ListCacheNames.FuelSurchargeCalculation;

        public FuelSurchargeCalculationListCache(
            IRepository<FuelSurchargeCalculation> repository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _repository = repository;
        }

        protected override async Task<List<FuelSurchargeCalculationCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _repository.GetQueryAsync(), afterDateTime)
                .Select(x => new FuelSurchargeCalculationCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    Name = x.Name,
                    Type = x.Type,
                    BaseFuelCost = x.BaseFuelCost,
                    CanChangeBaseFuelCost = x.CanChangeBaseFuelCost,
                    FreightRatePercent = x.FreightRatePercent,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(FuelSurchargeCalculation entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
