using System;

namespace DispatcherWeb.Orders.Dto
{
    public class GetPaymentReconciliationReportInput
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AllOffices { get; set; }
    }
}
