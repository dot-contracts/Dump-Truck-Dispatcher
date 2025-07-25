using System;
using DispatcherWeb.Caching;
using DispatcherWeb.Orders.Dto;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketCacheItem : AuditableCacheItem
    {
        public int? OrderLineId { get; set; }
        public int? LoadId { get; set; }
        public int? TruckId { get; set; }
        public int? DriverId { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public int? FreightUomId { get; set; }
        public int? MaterialUomId { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public decimal FuelSurcharge { get; set; }

        public TicketQuantityDto ToTicketQuantityDto(OrderLineCacheItem orderLine)
        {
            var ticket = this;
            return new TicketQuantityDto
            {
                TicketId = ticket.Id,
                Designation = orderLine.Designation,
                OrderLineFreightUomId = orderLine.FreightUomId,
                OrderLineMaterialUomId = orderLine.MaterialUomId,
                FuelSurcharge = ticket.FuelSurcharge,
                FreightQuantity = ticket.FreightQuantity,
                MaterialQuantity = ticket.MaterialQuantity,
                TicketUomId = ticket.FreightUomId,
            };
        }
    }
}
