using System;

namespace DispatcherWeb.JobSummary.Dto
{
    public class TripCycleDto
    {
        public string CycleId { get; set; }
        public int? LoadId { get; set; }
        public int? DriverId { get; set; }
        public int? TicketId { get; set; }
        public decimal? Quantity { get; set; }
        public string DriverName { get; set; }
        public string Location { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public TruckTripTypes TruckTripType { get; set; }
        public string Label { get; set; }
        public string SegmentHoverText { get; set; }
        public double? SourceLatitude { get; set; }
        public double? SourceLongitude { get; set; }

        public TimeSpan? Duration =>
            StartDateTime.HasValue ?
            (EndDateTime ?? StartDateTime).Value.Subtract(StartDateTime.Value) :
            default;

        public override string ToString()
        {
            return $"Label: {Label} Id: {CycleId} Type: {TruckTripType} Driver: {DriverId} Time: {(StartDateTime.HasValue ? StartDateTime.Value.ToString("hh:mm:ss") : "~")} -> {(EndDateTime.HasValue ? EndDateTime.Value.ToString("hh:mm:ss") : "~")}";
        }
    }
}


