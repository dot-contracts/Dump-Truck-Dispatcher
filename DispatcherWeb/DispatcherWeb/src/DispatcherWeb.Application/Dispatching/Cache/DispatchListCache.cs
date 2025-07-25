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
    public class DispatchListCache : ListCacheBase<ListCacheDateKey, DispatchCacheItem, Dispatch>,
        IDispatchListCache,
        ISingletonDependency
    {
        private readonly IRepository<Dispatch> _dispatchRepository;
        public override string CacheName => ListCacheNames.Dispatch;

        public DispatchListCache(
            IRepository<Dispatch> dispatchRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _dispatchRepository = dispatchRepository;
        }

        protected override async Task<List<DispatchCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _dispatchRepository.GetQueryAsync(), afterDateTime)
                .Where(x => x.OrderLine.Order.TenantId == key.TenantId
                    && x.OrderLine.Order.DeliveryDate == key.Date
                    && (x.OrderLine.Order.Shift == key.Shift || key.Shift == null))
                .Select(x => new DispatchCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    OrderLineId = x.OrderLineId,
                    OrderLineTruckId = x.OrderLineTruckId,
                    TruckId = x.TruckId,
                    Status = x.Status,
                    Acknowledged = x.Acknowledged,
                    IsMultipleLoads = x.IsMultipleLoads,
                })
                .ToListAsync();
        }

        protected override async Task<ListCacheDateKey> GetKeyFromEntity(Dispatch entity)
        {
            if (await DateKeyLookup.IsEnabled())
            {
                return await DateKeyLookup.GetKeyForOrderLine(entity.OrderLineId);
            }

            var keyData = await WithUnitOfWorkAsync(async () =>
            {
                return await (await _dispatchRepository.GetQueryAsync())
                    .Where(x => x.Id == entity.Id)
                    .Select(x => new
                    {
                        x.OrderLine.Order.DeliveryDate,
                        x.OrderLine.Order.Shift,
                    }).FirstAsync();
            });

            return new ListCacheDateKey(entity.TenantId, keyData.DeliveryDate, keyData.Shift);
        }
    }
}
