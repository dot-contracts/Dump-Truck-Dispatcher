using DispatcherWeb.Dto;

namespace DispatcherWeb.Drivers.Dto
{
    public class GetDriversSelectListInput : GetSelectListInput
    {
        //specify OrderLineId to filter drivers by Date/Shift/Office of that OrderLine.Order and Truck
        public int? OrderLineId { get; set; }

        public int? TruckId { get; set; }

        public string TruckCode { get; set; }

        public bool IncludeLeaseHaulerDrivers { get; set; }

        public int? OfficeId { get; set; }

        public int? LeaseHaulerId { get; set; }

        public OrderNotifyPreferredFormat[] OrderNotifyPreferredFormats { get; set; }
    }
}
