namespace DispatcherWeb.Dispatching.Dto
{
    public class CancelDispatchForDriverInput
    {
        public int DispatchId { get; set; }
        public DriverApplicationActionInfo Info { get; set; }
    }
}
