using System;
using System.Collections.Generic;

namespace DispatcherWeb.Orders.RevenueBreakdownReport.Dto
{
    public class FillDriversTimeInput
    {
        public DateTime DeliveryDateBegin { get; set; }
        public DateTime DeliveryDateEnd { get; set; }
        public List<int?> TruckIds { get; set; }
        public List<long> UserIds { get; set; }
        public bool ExcludeTimeWithPayStatements { get; set; }
        public bool LocalEmployeesOnly { get; set; }
    }
}
