using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class CompleteDispatchDto
    {
        public DriverApplicationActionInfo Info { get; set; }
        public int? Id { get; set; }
        public Guid? Guid { get; set; } //deprecated, temporarily kept for backwards compatibility
        public bool? IsMultipleLoads { get; set; }
        public bool? ContinueMultiload { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }

    }
}
