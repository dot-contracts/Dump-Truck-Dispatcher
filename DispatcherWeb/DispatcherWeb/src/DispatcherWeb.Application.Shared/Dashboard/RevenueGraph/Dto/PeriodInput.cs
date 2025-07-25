using System;
using DispatcherWeb.Dashboard.Dto;

namespace DispatcherWeb.Dashboard.RevenueGraph.Dto
{
    public class PeriodInput
    {
        public DateTime PeriodBegin { get; set; }
        public DateTime PeriodEnd { get; set; }
        public TicketType TicketType { get; set; }
        public int? OfficeId { get; set; }

    }
}
