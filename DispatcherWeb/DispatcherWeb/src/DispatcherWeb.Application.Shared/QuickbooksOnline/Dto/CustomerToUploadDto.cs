namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class CustomerToUploadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public bool IsInQuickBooks { get; set; }
        public string InvoiceEmail { get; set; }
        public InvoicingMethodEnum InvoicingMethod { get; set; }
        public PreferredBillingDeliveryMethodEnum? PreferredDeliveryMethod { get; set; }
        public PhysicalAddressDto BillingAddress { get; set; }
        public PhysicalAddressDto ShippingAddress { get; set; }
    }
}
