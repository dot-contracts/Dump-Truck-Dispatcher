namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class SendLeaseHaulerContactDto
    {
        public int TenantId { get; set; }
        public int LeaseHaulerId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CellPhoneNumber { get; set; }
        public OrderNotifyPreferredFormat NotifyPreferredFormat { get; set; }
    }
}
