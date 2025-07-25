namespace DispatcherWeb.Dispatching.Dto
{
    public class SendSmsOrEmailInput
    {
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public int DriverId { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }
        public bool SendOrdersToDriversImmediately { get; set; }
        public bool AfterCompleted { get; set; }
        public bool SkipIfDispatchesExist { get; set; } = true;
        public virtual bool? ActiveDispatchWasChanged { get; set; }
        public long? UserId { get; set; }
        public int? DispatchId { get; set; }
    }
}
