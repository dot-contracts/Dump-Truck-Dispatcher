using System;

namespace DispatcherWeb.Dashboard.Dto
{
    public class TruckActivePeriod
    {
        public DateTime? ActiveFrom { get; set; }
        public DateTime ActiveTo { get; set; }
        public int Number { get; set; }
    }
}
