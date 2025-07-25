using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using DispatcherWeb.Caching;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Tickets.Cache
{
    public class TicketListCache : ListCacheBase<ListCacheDateKey, TicketCacheItem, Ticket>,
        ITicketListCache,
        ISingletonDependency
    {
        private readonly IRepository<Ticket> _ticketRepository;
        public override string CacheName => ListCacheNames.Ticket;

        public TicketListCache(
            IRepository<Ticket> ticketRepository,
            ListCacheBaseDependency listCacheBaseDependency
        ) : base(listCacheBaseDependency)
        {
            _ticketRepository = ticketRepository;
        }

        protected override async Task<List<TicketCacheItem>> GetAllItemsFromDb(ListCacheDateKey key, DateTime? afterDateTime = null)
        {
            return await ApplyDateFilter(await _ticketRepository.GetQueryAsync(), afterDateTime)
                .Where(x => x.OrderLine.Order.TenantId == key.TenantId
                            && x.OrderLine.Order.DeliveryDate == key.Date
                            && (x.OrderLine.Order.Shift == key.Shift || key.Shift == null))
                .Select(x => new TicketCacheItem
                {
                    Id = x.Id,
                    IsDeleted = x.IsDeleted,
                    DeletionTime = x.DeletionTime,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    OrderLineId = x.OrderLineId,
                    LoadId = x.LoadId,
                    TruckId = x.TruckId,
                    DriverId = x.DriverId,
                    FreightQuantity = x.FreightQuantity,
                    MaterialQuantity = x.MaterialQuantity,
                    FreightUomId = x.FreightUomId,
                    MaterialUomId = x.MaterialUomId,
                    FuelSurcharge = x.FuelSurcharge,
                    TicketDateTime = x.TicketDateTime,
                })
                .ToListAsync();
        }

        protected override async Task<ListCacheDateKey> GetKeyFromEntity(Ticket entity)
        {
            if (entity.OrderLineId == null)
            {
                return null;
            }

            if (await DateKeyLookup.IsEnabled())
            {
                return await DateKeyLookup.GetKeyForOrderLine(entity.OrderLineId.Value);
            }

            var keyData = await WithUnitOfWorkAsync(entity, async () =>
            {
                return await (await _ticketRepository.GetQueryAsync())
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
