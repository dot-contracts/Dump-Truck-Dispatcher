using System;

namespace DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime DeliveryDate { get; set; }
        public CustomerDto Customer { get; set; }
        public string Directions { get; set; }
        public bool IsClosed { get; set; }
        public bool IsPending { get; set; }
        public string PONumber { get; set; }
        public OrderPriority Priority { get; set; }
        public string SpectrumNumber { get; set; }
    }
}
