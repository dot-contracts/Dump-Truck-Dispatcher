using System;

namespace DispatcherWeb.Orders.Dto
{
    public class GetOrderDuplicateCountInput
    {
        public int? Id { get; set; }

        public int? CustomerId { get; set; }

        public int? QuoteId { get; set; }

        public DateTime? DeliveryDate { get; set; }

    }
}
