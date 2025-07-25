using System;
using System.Collections.Generic;
using System.Globalization;

namespace DispatcherWeb.Invoices.Dto
{
    public class InvoicePrintOutDto
    {
        public string CustomerName { get; set; }
        public BillingTermsEnum? Terms { get; set; }
        public int? OfficeId { get; set; }
        public string BillingAddress { get; set; }
        //public string CustomerBillingAddress1 { get; set; }
        //public string CustomerBillingAddress2 { get; set; }
        //public string CustomerBillingCity { get; set; }
        //public string CustomerBillingState { get; set; }
        //public string CustomerBillingZipCode { get; set; }
        public string JobNumber { get; set; }
        public string PoNumber { get; set; }
        public string Description { get; set; }
        public int InvoiceId { get; set; }
        public string NumberPrefix { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public string Message { get; set; }
        public bool CombineTickets { get; set; }

        public List<InvoicePrintOutLineItemDto> InvoiceLines { get; set; }


        public string LegalName { get; set; }
        public string LegalAddress { get; set; }
        public string LegalPhoneNumber { get; set; }
        public string RemitToInformation { get; set; }
        public byte[] LogoBytes { get; set; }
        public string TimeZone { get; set; }
        public CultureInfo CurrencyCulture { get; set; }
        public string TermsAndConditions { get; set; }
        public string CompanyName { get; set; }

        public bool DebugLayout { get; set; }
        public GetInvoicePrintOutInput DebugInput { get; set; }
    }
}
