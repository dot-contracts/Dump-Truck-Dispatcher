using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Orders.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders.Cache
{
    public class OrderLineVehicleCategoryListCache : ListCacheBase<ListCacheDateKey, OrderLineVehicleCategoryCacheItem, OrderLineVehicleCategory>,
        IOrderLineVehicleCategoryListCache,
        ISingletonDependency
    {
        private readonly IRepository<OrderLineVehicleCategory> _orderLineVehicleRepository;
        public override string CacheName => ListCacheNames.OrderLineVehicleCategory;

        public OrderLineVehicleCategoryListCache(
            IRepository<OrderLineVehicleCategory> orderLineVehicleRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _orderLineVehicleRepository = orderLineVehicleRepository;
        }

        protected override async Task<List<OrderLineVehicleCategoryCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _orderLineVehicleRepository.GetQueryAsync(), afterDateTime)
                .Where(x => x.OrderLine.Order.TenantId == key.TenantId
                            && x.OrderLine.Order.DeliveryDate == key.Date
                            && (x.OrderLine.Order.Shift == key.Shift || key.Shift == null))
                .Select(x => new OrderLineVehicleCategoryCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    OrderLineId = x.OrderLineId,
                    VehicleCategoryId = x.VehicleCategoryId,
                })
                .ToListAsync();
        }

        protected override async Task<ListCacheDateKey> GetKeyFromEntity(OrderLineVehicleCategory entity)
        {
            if (await DateKeyLookup.IsEnabled())
            {
                return await DateKeyLookup.GetKeyForOrderLine(entity.OrderLineId);
            }

            var keyData = await WithUnitOfWorkAsync(entity, async () =>
            {
                return await (await _orderLineVehicleRepository.GetQueryAsync())
                    .Where(x => x.Id == entity.Id)
                    .Select(x => new
                    {
                        x.OrderLine.Order.DeliveryDate,
                        x.OrderLine.Order.Shift,
                    }).FirstAsync();
            });

            var key = new ListCacheDateKey(entity.TenantId, keyData.DeliveryDate, keyData.Shift);

            return key;
        }
    }
}
