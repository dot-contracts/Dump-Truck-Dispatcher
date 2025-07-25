using System;

namespace DispatcherWeb.Charges.Dto
{
    public class ChargeOrderLineDetailsDto
    {
        public DateTime DeliveryDate { get; set; }

        public string CustomerName { get; set; }

        public string ItemName { get; set; }

        public string MaterialItemName { get; set; }

        public string LoadAtName { get; set; }

        public string DeliverToName { get; set; }
    }
}
