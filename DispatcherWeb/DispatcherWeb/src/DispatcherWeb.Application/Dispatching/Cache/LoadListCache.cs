using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Dispatching.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dispatching.Cache
{
    public class LoadListCache : ListCacheBase<ListCacheDateKey, LoadCacheItem, Load>,
        ILoadListCache,
        ISingletonDependency
    {
        private readonly IRepository<Load> _loadRepository;
        public override string CacheName => ListCacheNames.Load;

        public LoadListCache(
            IRepository<Load> loadRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _loadRepository = loadRepository;
        }

        protected override async Task<List<LoadCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _loadRepository.GetQueryAsync(), afterDateTime)
                .Where(x => x.Dispatch.OrderLine.Order.TenantId == key.TenantId
                            && x.Dispatch.OrderLine.Order.DeliveryDate == key.Date
                            && (x.Dispatch.OrderLine.Order.Shift == key.Shift || key.Shift == null))
                .Select(x => new LoadCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    DispatchId = x.DispatchId,
                    SourceDateTime = x.SourceDateTime,
                    DestinationDateTime = x.DestinationDateTime,
                })
                .ToListAsync();
        }

        protected override async Task<ListCacheDateKey> GetKeyFromEntity(Load entity)
        {
            if (await DateKeyLookup.IsEnabled())
            {
                return await DateKeyLookup.GetKeyForDispatch(entity.DispatchId);
            }

            var keyData = await WithUnitOfWorkAsync(entity, async () =>
            {
                return await (await _loadRepository.GetQueryAsync())
                    .Where(x => x.Id == entity.Id)
                    .Select(x => new
                    {
                        x.Dispatch.OrderLine.Order.DeliveryDate,
                        x.Dispatch.OrderLine.Order.Shift,
                    }).FirstAsync();
            });

            var key = new ListCacheDateKey(entity.TenantId, keyData.DeliveryDate, keyData.Shift);

            return key;
        }
    }
}
