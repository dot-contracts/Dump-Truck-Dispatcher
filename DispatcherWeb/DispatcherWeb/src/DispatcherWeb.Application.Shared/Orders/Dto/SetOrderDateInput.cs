using System;

namespace DispatcherWeb.Orders.Dto
{
    public class SetOrderDateInput
    {
        public int OrderId { get; set; }
        public int? OrderLineId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public bool KeepTrucks { get; set; }

        public bool RemoveNotAvailableTrucks { get; set; }
    }
}
