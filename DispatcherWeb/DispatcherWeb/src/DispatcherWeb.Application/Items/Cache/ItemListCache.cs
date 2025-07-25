using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Items.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Items.Cache
{
    public class ItemListCache : ListCacheBase<ListCacheTenantKey, ItemCacheItem, Item>,
        IItemListCache,
        ISingletonDependency
    {
        private readonly IRepository<Item> _itemRepository;
        public override string CacheName => ListCacheNames.Item;

        public ItemListCache(
            IRepository<Item> itemRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _itemRepository = itemRepository;
        }

        protected override async Task<List<ItemCacheItem>> GetAllItemsFromDb(ListCacheTenantKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _itemRepository.GetQueryAsync(), afterDateTime)
                .Select(i => new ItemCacheItem
                {
                    Id = i.Id,
                    IsDeleted = i.IsDeleted,
                    DeletionTime = i.DeletionTime,
                    CreationTime = i.CreationTime,
                    LastModificationTime = i.LastModificationTime,
                    Name = i.Name,
                    IsActive = i.IsActive,
                    Type = i.Type,
                    IsTaxable = i.IsTaxable,
                    UseZoneBasedRates = i.UseZoneBasedRates,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheTenantKey> GetKeyFromEntity(Item entity)
        {
            return Task.FromResult(new ListCacheTenantKey(entity.TenantId));
        }
    }
}
