namespace DispatcherWeb.Dispatching.Dto
{
    public class AcknowledgeDispatchInput
    {
        public int DispatchId { get; set; }
        public DriverApplicationActionInfo Info { get; set; }
    }
}
