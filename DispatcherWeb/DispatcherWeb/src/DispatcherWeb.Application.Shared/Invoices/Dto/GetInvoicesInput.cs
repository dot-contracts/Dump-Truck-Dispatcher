using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Invoices.Dto
{
    public class GetInvoicesInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int? CustomerId { get; set; }
        public bool ApplyCustomerLimitations { get; set; }
        public InvoiceStatus? Status { get; set; }
        public DateTime? IssueDateStart { get; set; }
        public DateTime? IssueDateEnd { get; set; }
        public int? OfficeId { get; set; }
        public int? BatchId { get; set; }
        public int? UploadBatchId { get; set; }
        public string TicketNumber { get; set; }
        public int? InvoiceId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "CustomerName";
            }
        }
    }
}
