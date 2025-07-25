using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Invoices.Dto
{
    public class GetInvoiceLinesInput : SortedInputDto, IShouldNormalize
    {
        public int InvoiceId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Id";
            }
        }
    }
}
