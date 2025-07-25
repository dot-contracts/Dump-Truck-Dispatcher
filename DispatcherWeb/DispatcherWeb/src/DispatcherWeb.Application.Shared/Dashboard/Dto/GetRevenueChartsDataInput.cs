using System;

namespace DispatcherWeb.Dashboard.Dto
{
    public class GetRevenueChartsDataInput
    {
        public TicketType TicketType { get; set; }
        public DateTime PeriodBegin { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
