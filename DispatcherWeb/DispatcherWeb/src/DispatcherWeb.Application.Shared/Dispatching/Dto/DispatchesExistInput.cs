namespace DispatcherWeb.Dispatching.Dto
{
    public class DispatchesExistInput
    {
        public int? OrderLineId { get; set; }
        public DispatchStatus[] DispatchStatuses { get; set; }
        public bool? IsMultipleLoads { get; set; }
    }
}
