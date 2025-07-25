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
    public class CustomerListCache : ListCacheBase<ListCacheTenantKey, CustomerCacheItem, Customer>,
        ICustomerListCache,
        ISingletonDependency
    {
        private readonly IRepository<Customer> _customerRepository;
        public override string CacheName => ListCacheNames.Customer;

        public CustomerListCache(
            IRepository<Customer> customerRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _customerRepository = customerRepository;
        }

        protected override async Task<List<CustomerCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _customerRepository.GetQueryAsync(), afterDateTime)
                .Select(c => new CustomerCacheItem
                {
                    Id = c.Id,
                    IsDeleted = c.IsDeleted,
                    DeletionTime = c.DeletionTime,
                    CreationTime = c.CreationTime,
                    LastModificationTime = c.LastModificationTime,
                    Name = c.Name,
                    IsCod = c.IsCod,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(Customer entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
