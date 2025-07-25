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
    public class OrderListCache : ListCacheBase<ListCacheDateKey, OrderCacheItem, Order>,
        IOrderListCache,
        ISingletonDependency
    {
        private readonly IRepository<Order> _orderRepository;
        public override string CacheName => ListCacheNames.Order;

        public OrderListCache(
            IRepository<Order> orderRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _orderRepository = orderRepository;
        }

        protected override async Task<List<OrderCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _orderRepository.GetQueryAsync(), afterDateTime)
                .Where(o => o.TenantId == key.TenantId
                    && o.DeliveryDate == key.Date
                    && (o.Shift == key.Shift || key.Shift == null))
                .Select(o => new OrderCacheItem
                {
                    Id = o.Id,
                    IsDeleted = o.IsDeleted,
                    DeletionTime = o.DeletionTime,
                    CreationTime = o.CreationTime,
                    LastModificationTime = o.LastModificationTime,
                    DeliveryDate = o.DeliveryDate,
                    Shift = o.Shift,
                    Priority = o.Priority,
                    OfficeId = o.OfficeId,
                    CustomerId = o.CustomerId,
                    PoNumber = o.PONumber,
                    SpectrumNumber = o.SpectrumNumber,
                    ChargeTo = o.ChargeTo,
                    Directions = o.Directions,
                    IsPending = o.IsPending,
                    SalesTaxRate = o.SalesTaxRate,
                    SalesTax = o.SalesTax,
                    FreightTotal = o.FreightTotal,
                    MaterialTotal = o.MaterialTotal,
                    CodTotal = o.CODTotal,
                })
                .ToListAsync();
        }

        protected override Task<ListCacheDateKey> GetKeyFromEntity(Order entity)
        {
            var key = new ListCacheDateKey(entity.TenantId, entity.DeliveryDate, entity.Shift);

            return Task.FromResult(key);
        }
    }
}
