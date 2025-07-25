namespace DispatcherWeb.Scheduling.Dto
{
    public class ReassignTrucksInput
    {
        public int SourceOrderLineId { get; set; }
        public int DestinationOrderLineId { get; set; }
        public int[] TruckIds { get; set; }
    }
}
