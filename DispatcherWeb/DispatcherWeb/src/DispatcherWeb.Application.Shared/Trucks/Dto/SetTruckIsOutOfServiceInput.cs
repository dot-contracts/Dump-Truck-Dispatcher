using System;

namespace DispatcherWeb.Trucks.Dto
{
    public class SetTruckIsOutOfServiceInput
    {
        public int TruckId { get; set; }
        public DateTime Date { get; set; }
        public bool IsOutOfService { get; set; }
        public string Reason { get; set; }
    }
}
