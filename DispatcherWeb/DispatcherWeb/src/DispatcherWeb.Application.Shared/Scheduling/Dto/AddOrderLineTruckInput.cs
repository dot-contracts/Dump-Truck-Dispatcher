namespace DispatcherWeb.Scheduling.Dto
{
    public class AddOrderLineTruckInput
    {
        public int OrderLineId { get; set; }
        public int TruckId { get; set; }
        public int? DriverId { get; set; }
        public int? ParentId { get; set; }
        public int? TrailerId { get; set; }
    }
}
