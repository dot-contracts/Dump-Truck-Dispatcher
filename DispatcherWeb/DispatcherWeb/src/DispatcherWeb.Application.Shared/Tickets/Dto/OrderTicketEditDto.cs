using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Tickets.Dto
{
    public class OrderTicketEditDto : ITicketEditQuantity
    {
        public int OrderLineId { get; set; }

        public int Id { get; set; }

        [StringLength(EntityStringFieldLengths.Ticket.TicketNumber)]
        public string TicketNumber { get; set; }

        public DateTime? TicketDateTime { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public int? FreightUomId { get; set; }
        public int? FreightItemId { get; set; }
        public int? MaterialItemId { get; set; }
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? TrailerId { get; set; }
        public string TrailerTruckCode { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
    }
}
