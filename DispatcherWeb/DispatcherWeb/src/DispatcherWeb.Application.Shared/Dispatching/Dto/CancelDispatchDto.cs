namespace DispatcherWeb.Dispatching.Dto
{
    public class CancelDispatchDto
    {
        public int DispatchId { get; set; }

        public bool CancelAllDispatchesForDriver { get; set; }
    }
}
