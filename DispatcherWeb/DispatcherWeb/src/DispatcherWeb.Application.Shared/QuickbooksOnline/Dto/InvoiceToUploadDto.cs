using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Extensions;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class InvoiceToUploadDto<TInvoice> //DispatcherWeb.Invoices.Invoice isn't accessible from this layer
    {
        public TInvoice Invoice { get; set; }
        public int InvoiceId { get; set; }
        public string BillingAddress { get; set; }
        public string EmailAddress { get; set; }
        public CustomerToUploadDto Customer { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public string Message { get; set; }
        public string PONumber { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxRate { get; set; }
        public string TaxName { get; set; }
        public decimal TotalAmount { get; set; }
        public BillingTermsEnum? Terms { get; set; }
        public InvoiceStatus Status { get; set; }
        public string OfficeName { get; set; }
        public List<InvoiceLineToUploadDto> InvoiceLines { get; set; }

        public string GetJobNumberOnly()
        {
            if (InvoiceLines == null || Customer == null)
            {
                return null;
            }

            var jobNumbers = InvoiceLines.Select(x => x.JobNumber).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (Customer.InvoicingMethod.IsIn(InvoicingMethodEnum.SeparateTicketsByJobNumber, InvoicingMethodEnum.SeparateTicketsByJob) && jobNumbers.Count <= 1)
            {
                return jobNumbers.FirstOrDefault();
            }

            return null;
        }

        public string GetPoNumberOrJobNumber()
        {
            var jobNumber = GetJobNumberOnly();
            if (!string.IsNullOrEmpty(jobNumber))
            {
                return jobNumber;
            }

            return PONumber;
        }
    }
}
