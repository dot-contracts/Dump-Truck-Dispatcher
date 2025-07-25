using Abp.Application.Services.Dto;

namespace DispatcherWeb.Receipts.Dto
{
    public class GetReceiptForEditInput : NullableIdDto
    {
        public GetReceiptForEditInput()
        {
        }

        public GetReceiptForEditInput(int? id, int? orderId)
            : base(id)
        {
            OrderId = orderId;
        }

        public int? OrderId { get; set; }
    }
}
