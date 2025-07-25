namespace DispatcherWeb.Dispatching.Dto
{
    public class MarkDispatchCompleteInput
    {
        public int DispatchId { get; set; }
        public DriverApplicationActionInfo Info { get; set; }
    }
}
