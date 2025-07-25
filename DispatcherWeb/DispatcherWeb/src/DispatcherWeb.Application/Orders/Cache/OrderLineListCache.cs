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
    public class OrderLineListCache : ListCacheBase<ListCacheDateKey, OrderLineCacheItem, OrderLine>,
        IOrderLineListCache,
        ISingletonDependency
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        public override string CacheName => ListCacheNames.OrderLine;

        public OrderLineListCache(
            IRepository<OrderLine> orderLineRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _orderLineRepository = orderLineRepository;
        }

        protected override async Task<List<OrderLineCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _orderLineRepository.GetQueryAsync(), afterDateTime)
                .Where(ol => ol.Order.TenantId == key.TenantId
                    && ol.Order.DeliveryDate == key.Date
                    && (ol.Order.Shift == key.Shift || key.Shift == null))
                .Select(ol => new OrderLineCacheItem
                {
                    Id = ol.Id,
                    IsDeleted = ol.IsDeleted,
                    DeletionTime = ol.DeletionTime,
                    CreationTime = ol.CreationTime,
                    LastModificationTime = ol.LastModificationTime,
                    OrderId = ol.OrderId,
                    IsTimeStaggered = ol.StaggeredTimeKind != StaggeredTimeKind.None,
                    IsTimeEditable = ol.StaggeredTimeKind == StaggeredTimeKind.None,
                    Time = ol.StaggeredTimeKind == StaggeredTimeKind.SetInterval ? ol.FirstStaggeredTimeOnJob : ol.TimeOnJob,
                    TimeOnJob = ol.TimeOnJob,
                    StaggeredTimeKind = ol.StaggeredTimeKind,
                    FirstStaggeredTimeOnJob = ol.FirstStaggeredTimeOnJob,
                    StaggeredTimeInterval = ol.StaggeredTimeInterval,
                    LoadAtId = ol.LoadAtId,
                    DeliverToId = ol.DeliverToId,
                    JobNumber = ol.JobNumber,
                    Note = ol.Note,
                    MaterialItemId = ol.MaterialItemId,
                    FreightItemId = ol.FreightItemId,
                    MaterialUomId = ol.MaterialUomId,
                    FreightUomId = ol.FreightUomId,
                    MaterialQuantity = ol.MaterialQuantity,
                    FreightQuantity = ol.FreightQuantity,
                    IsFreightPriceOverridden = ol.IsFreightPriceOverridden,
                    IsMaterialPriceOverridden = ol.IsMaterialPriceOverridden,
                    Designation = ol.Designation,
                    NumberOfTrucks = ol.NumberOfTrucks,
                    ScheduledTrucks = ol.ScheduledTrucks,
                    IsComplete = ol.IsComplete,
                    IsCancelled = ol.IsCancelled,
                    HaulingCompanyOrderLineId = ol.HaulingCompanyOrderLineId,
                    MaterialCompanyOrderLineId = ol.MaterialCompanyOrderLineId,
                })
                .ToListAsync();
        }

        protected override async Task<ListCacheDateKey> GetKeyFromEntity(OrderLine entity)
        {
            if (await DateKeyLookup.IsEnabled())
            {
                return await DateKeyLookup.GetKeyForOrder(entity.OrderId);
            }

            var keyData = await WithUnitOfWorkAsync(entity, async () =>
            {
                return await (await _orderLineRepository.GetQueryAsync())
                    .Where(ol => ol.Id == entity.Id)
                    .Select(ol => new
                    {
                        ol.TenantId,
                        ol.Order.DeliveryDate,
                        ol.Order.Shift,
                    })
                    .FirstOrDefaultAsync();
            });

            var key = new ListCacheDateKey(keyData.TenantId, keyData.DeliveryDate, keyData.Shift);

            return key;
        }
    }
}
