namespace DispatcherWeb.Invoices.Dto
{
    public class CustomerSelectListInfoDto
    {
        public int CustomerId { get; set; }

        public string InvoiceEmail { get; set; }

        public string BillingAddress1 { get; set; }

        public string BillingAddress2 { get; set; }

        public string BillingCity { get; set; }

        public string BillingState { get; set; }

        public string BillingZipCode { get; set; }

        public string FullAddress => Utilities.FormatAddress2(BillingAddress1, BillingAddress2, BillingCity, BillingState, BillingZipCode);

        public BillingTermsEnum? Terms { get; set; }

        public InvoicingMethodEnum InvoicingMethod { get; set; }

    }
}
