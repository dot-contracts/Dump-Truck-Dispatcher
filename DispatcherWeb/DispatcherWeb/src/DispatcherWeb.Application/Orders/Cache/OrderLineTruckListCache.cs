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
    public class OrderLineTruckListCache : ListCacheBase<ListCacheDateKey, OrderLineTruckCacheItem, OrderLineTruck>,
        IOrderLineTruckListCache,
        ISingletonDependency
    {
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        public override string CacheName => ListCacheNames.OrderLineTruck;

        public OrderLineTruckListCache(
            IRepository<OrderLineTruck> orderLineTruckRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _orderLineTruckRepository = orderLineTruckRepository;
        }

        protected override async Task<List<OrderLineTruckCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _orderLineTruckRepository.GetQueryAsync(), afterDateTime)
                .Where(olt => olt.OrderLine.Order.TenantId == key.TenantId
                    && olt.OrderLine.Order.DeliveryDate == key.Date
                    && (olt.OrderLine.Order.Shift == key.Shift || key.Shift == null))
                .Select(olt => new OrderLineTruckCacheItem
                {
                    Id = olt.Id,
                    IsDeleted = olt.IsDeleted,
                    DeletionTime = olt.DeletionTime,
                    CreationTime = olt.CreationTime,
                    LastModificationTime = olt.LastModificationTime,
                    ParentOrderLineTruckId = olt.ParentOrderLineTruckId,
                    TruckId = olt.TruckId,
                    TrailerId = olt.TrailerId,
                    DriverId = olt.DriverId,
                    OrderLineId = olt.OrderLineId,
                    Utilization = olt.Utilization,
                    IsDone = olt.IsDone,
                    TimeOnJob = olt.TimeOnJob,
                })
                .ToListAsync();
        }

        protected override async Task<ListCacheDateKey> GetKeyFromEntity(OrderLineTruck entity)
        {
            if (await DateKeyLookup.IsEnabled())
            {
                return await DateKeyLookup.GetKeyForOrderLine(entity.OrderLineId);
            }

            var keyData = await WithUnitOfWorkAsync(entity, async () =>
            {
                return await (await _orderLineTruckRepository.GetQueryAsync())
                    .Where(olt => olt.Id == entity.Id)
                    .Select(olt => new
                    {
                        olt.TenantId,
                        olt.OrderLine.Order.DeliveryDate,
                        olt.OrderLine.Order.Shift,
                    })
                    .FirstOrDefaultAsync();
            });

            var key = new ListCacheDateKey(keyData.TenantId, keyData.DeliveryDate, keyData.Shift);

            return key;
        }
    }
}
