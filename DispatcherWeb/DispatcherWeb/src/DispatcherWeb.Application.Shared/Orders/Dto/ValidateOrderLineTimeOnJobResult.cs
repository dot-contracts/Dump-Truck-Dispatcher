namespace DispatcherWeb.Orders.Dto
{
    public class ValidateOrderLineTimeOnJobResult
    {
        public bool HasOrderLineTrucks { get; set; }
        public bool HasDisagreeingOrderLineTrucks { get; set; }
        public bool HasOpenDispatches { get; set; }
    }
}
