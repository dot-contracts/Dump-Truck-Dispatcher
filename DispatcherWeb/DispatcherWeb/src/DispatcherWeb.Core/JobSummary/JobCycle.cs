using System;

namespace DispatcherWeb.JobSummary
{
    public class JobCycle
    {
        public int LoadId { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public double? SourceLatitude { get; set; }
        public double? SourceLongitude { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public string LocationName { get; set; }
        public string LocationStreetAddress { get; set; }
        public string LocationCity { get; set; }
        public string LocationState { get; set; }
        public string LocationNameFormatted => Utilities.ConcatenateAddress(LocationName, LocationStreetAddress, LocationCity, LocationState, null);
        public TruckTripTypes TripType { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? TripStart { get; set; }
        public DateTime? TripEnd { get; set; }
        public TimeSpan TripDuration =>
                TripStart.HasValue
                    ? (TripEnd ?? TripStart).Value.Subtract(TripStart.Value)
                    : default;
        public int? TicketId { get; set; }
        public decimal TicketQuantity { get; set; }
        public int? TicketUomId { get; set; }
        public string TicketUom { get; set; }
    }
}
