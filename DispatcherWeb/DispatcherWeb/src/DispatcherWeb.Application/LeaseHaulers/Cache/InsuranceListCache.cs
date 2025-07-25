using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Insurances;
using DispatcherWeb.LeaseHaulers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulers.Cache
{
    public class InsuranceListCache : ListCacheBase<ListCacheTenantKey, InsuranceCacheItem, Insurance>,
        IInsuranceListCache,
        ISingletonDependency
    {
        private readonly IRepository<Insurance> _insuranceRepository;
        public override string CacheName => ListCacheNames.Insurance;

        public InsuranceListCache(
            IRepository<Insurance> insuranceRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _insuranceRepository = insuranceRepository;
        }

        protected override async Task<List<InsuranceCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _insuranceRepository.GetQueryAsync(), afterDateTime)
                .Select(x => new InsuranceCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    LeaseHaulerId = x.LeaseHaulerId,
                    ExpirationDate = x.ExpirationDate,
                    IsActive = x.IsActive,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(Insurance entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
