using System.Collections.Generic;
using System.Linq;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Orders.RevenueBreakdownReport.Dto
{
    public class RevenueBreakdownItemFromTickets : RevenueBreakdownItem
    {
        public List<TicketQuantityDto> Tickets { get; set; }

        public override int? TicketCount { get => Tickets?.Count; }

        public override decimal? ActualMaterialQuantity => Tickets.Sum(t => t.GetMaterialQuantity());

        public override decimal? ActualFreightQuantity => Tickets.Sum(t => t.GetFreightQuantity());

        public override decimal FuelSurcharge => Tickets.Sum(t => t.FuelSurcharge);
    }
}
