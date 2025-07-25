using System;

namespace DispatcherWeb.Dashboard.Dto
{
    public class TruckUtilizationOnDate
    {
        public DateTime DeliveryDate { get; set; }
        public decimal Utilization { get; set; }
    }
}
