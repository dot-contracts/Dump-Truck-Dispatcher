namespace DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender
{
    public class DriverDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsInactive { get; set; }
        public string EmailAddress { get; set; }
        public string CellPhoneNumber { get; set; }
        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }
    }
}
