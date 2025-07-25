namespace DispatcherWeb.Dispatching.Dto.DispatchSender
{
    public class DispatchEditDto : DispatchDto
    {
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public long? UserId { get; set; }
        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }
        public string Note { get; set; }
        public bool IsMultipleLoads { get; set; }
        public bool WasMultipleLoads { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }

    }
}
