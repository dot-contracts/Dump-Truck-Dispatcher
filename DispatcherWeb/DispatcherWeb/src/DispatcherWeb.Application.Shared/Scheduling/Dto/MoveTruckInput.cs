namespace DispatcherWeb.Scheduling.Dto
{
    public class MoveTruckInput
    {
        public int TruckId { get; set; }
        public int SourceOrderLineTruckId { get; set; }
        public int DestinationOrderLineId { get; set; }
    }
}
