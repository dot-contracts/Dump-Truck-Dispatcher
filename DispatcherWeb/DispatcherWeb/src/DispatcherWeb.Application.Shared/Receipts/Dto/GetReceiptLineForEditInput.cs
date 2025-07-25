using Abp.Application.Services.Dto;

namespace DispatcherWeb.Receipts.Dto
{
    public class GetReceiptLineForEditInput : NullableIdDto
    {
        public GetReceiptLineForEditInput()
        {
        }

        public GetReceiptLineForEditInput(int? id, int? receiptId)
            : base(id)
        {
            ReceiptId = receiptId;
        }

        public int? ReceiptId { get; set; }
    }
}
