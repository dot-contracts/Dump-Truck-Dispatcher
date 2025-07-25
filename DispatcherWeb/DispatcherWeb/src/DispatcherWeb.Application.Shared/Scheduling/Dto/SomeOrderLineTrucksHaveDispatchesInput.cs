namespace DispatcherWeb.Scheduling.Dto
{
    public class SomeOrderLineTrucksHaveDispatchesInput
    {
        public int OrderLineId { get; set; }
        public int[] TruckIds { get; set; }
    }
}
