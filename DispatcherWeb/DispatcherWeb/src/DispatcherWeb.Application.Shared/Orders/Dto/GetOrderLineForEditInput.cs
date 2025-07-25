using Abp.Application.Services.Dto;

namespace DispatcherWeb.Orders.Dto
{
    public class GetOrderLineForEditInput : NullableIdDto
    {
        public GetOrderLineForEditInput()
        {
        }

        public GetOrderLineForEditInput(int? id, int? orderId)
            : base(id)
        {
            OrderId = orderId;
        }

        public int? OrderId { get; set; }
    }
}
