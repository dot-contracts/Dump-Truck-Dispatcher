namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineLoadsInput
    {
        public int OrderLineId { get; set; }
        public int? Loads { get; set; }
    }
}
