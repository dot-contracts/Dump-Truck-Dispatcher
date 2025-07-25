using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Customers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Customers.Cache
{
    public class CustomerContactListCache : ListCacheBase<ListCacheTenantKey, CustomerContactCacheItem, CustomerContact>,
        ICustomerContactListCache,
        ISingletonDependency
    {
        private readonly IRepository<CustomerContact> _customerContactRepository;
        public override string CacheName => ListCacheNames.CustomerContact;

        public CustomerContactListCache(
            IRepository<CustomerContact> customerContactRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _customerContactRepository = customerContactRepository;
        }

        protected override async Task<List<CustomerContactCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _customerContactRepository.GetQueryAsync(), afterDateTime)
                .Select(c => new CustomerContactCacheItem
                {
                    Id = c.Id,
                    IsDeleted = c.IsDeleted,
                    DeletionTime = c.DeletionTime,
                    CreationTime = c.CreationTime,
                    LastModificationTime = c.LastModificationTime,
                    CustomerId = c.CustomerId,
                    HasCustomerPortalAccess = c.HasCustomerPortalAccess,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(CustomerContact entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
