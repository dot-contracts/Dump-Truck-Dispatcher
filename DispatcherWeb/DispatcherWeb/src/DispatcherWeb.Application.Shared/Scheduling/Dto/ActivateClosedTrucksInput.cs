namespace DispatcherWeb.Scheduling.Dto
{
    public class ActivateClosedTrucksInput
    {
        public int OrderLineId { get; set; }

        public int[] TruckIds { get; set; }
    }
}
