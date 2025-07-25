using System;
using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class PaymentReconciliationReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AllOffices { get; set; }
        public string OfficeName { get; set; }
        public List<PaymentReconciliationReportItemDto> Items { get; set; }
    }
}
