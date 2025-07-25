using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetTruckUtilizationInput
    {
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int TruckId { get; set; }
    }
}
