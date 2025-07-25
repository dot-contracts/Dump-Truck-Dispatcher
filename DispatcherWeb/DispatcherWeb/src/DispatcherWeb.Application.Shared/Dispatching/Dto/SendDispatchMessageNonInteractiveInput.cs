namespace DispatcherWeb.Dispatching.Dto
{
    public class SendDispatchMessageNonInteractiveInput
    {
        public int OrderLineId { get; set; }
        public int? SelectedOrderLineTruckId { get; set; }
    }
}
