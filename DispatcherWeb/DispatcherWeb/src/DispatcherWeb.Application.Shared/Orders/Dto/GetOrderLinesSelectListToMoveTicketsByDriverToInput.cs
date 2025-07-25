using System;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Orders.Dto
{
    public class GetOrderLinesSelectListToMoveTicketsByDriverToInput : GetSelectListInput
    {
        public DateTime DeliveryDate { get; set; }
        public int CustomerId { get; set; }
    }
}
