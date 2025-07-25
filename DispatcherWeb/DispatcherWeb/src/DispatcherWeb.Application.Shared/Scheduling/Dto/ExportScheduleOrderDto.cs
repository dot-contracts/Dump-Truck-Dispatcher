using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ExportScheduleOrderDto
    {
        public string Customer { get; set; }
        public string DriverName { get; set; }
        public string TruckCode { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime? TimeOnJobUtc { get; set; }
        public string TimeZone { get; set; }
        public string TimeOnJob => TimeOnJobUtc?.ConvertTimeZoneTo(TimeZone).ToString("h:mm tt");
        public string JobNumber { get; set; }
        public string StartName { get; set; }
        public string StartAddress { get; set; }
        public string DeliverTo { get; set; }
        public string DeliverToAddress { get; set; }
        public string FreightItemName { get; set; }
        public string MaterialItemName { get; set; }
        public string Contact { get; set; }
        public string AdditionalNotes { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        public string ChargeTo { get; set; }
    }
}
