namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class AvailableTruckUsageDto
    {
        public int TruckId { get; set; }
        public int? DriverId { get; set; }
        public bool IsTruckInUse => true;
        public bool IsDriverInUse => DriverId.HasValue;

        //public override bool Equals(object obj)
        //{
        //    if (obj is AvailableTruckUsageDto other && other != null)
        //    {
        //        return other.TruckId == TruckId && other.DriverId == DriverId;
        //    }
        //    return base.Equals(obj);
        //}
    }
}
