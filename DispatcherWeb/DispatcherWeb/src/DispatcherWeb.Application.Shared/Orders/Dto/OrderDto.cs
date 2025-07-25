using System;
using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderDto
    {
        public OrderDto()
        {
            EmailDeliveryStatuses = new List<EmailDeliveryStatus>();
        }
        public int Id { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string OfficeName { get; set; }
        public string CustomerName { get; set; }
        public string QuoteName { get; set; }
        public string PONumber { get; set; }
        public string ContactName { get; set; }
        public string ChargeTo { get; set; }
        public decimal CODTotal { get; set; }
        public double? NumberOfTrucks { get; set; }
        public List<EmailDeliveryStatus> EmailDeliveryStatuses { get; set; }

        public EmailDeliveryStatus? CalculatedEmailDeliveryStatus => EmailDeliveryStatuses.GetLowestStatus();

        public int OfficeId { get; set; }
    }
}
