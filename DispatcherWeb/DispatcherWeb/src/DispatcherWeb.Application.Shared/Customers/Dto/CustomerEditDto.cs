using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Customers.Dto
{
    public class CustomerEditDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(30)]
        public string AccountNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Customer.TaxIdNumber)]
        public string TaxIdNumber { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string Address1 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string Address2 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string City { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string State { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string ZipCode { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength)]
        public string CountryCode { get; set; }


        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string BillingAddress1 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string BillingAddress2 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string BillingCity { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string BillingState { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string BillingZipCode { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength)]
        public string BillingCountryCode { get; set; }

        [StringLength(EntityStringFieldLengths.General.Email)]
        public string InvoiceEmail { get; set; }

        public int? PricingTierId { get; set; }

        public string PricingTierName { get; set; }

        public PreferredBillingDeliveryMethodEnum? PreferredDeliveryMethod { get; set; }

        public BillingTermsEnum? Terms { get; set; }

        public bool IsActive { get; set; }

        public bool IsTaxExempt { get; set; }

        public bool CombineTickets { get; set; }

        [StringLength(EntityStringFieldLengths.Customer.Notes)]
        public string Notes { get; set; }

        public bool CustomerIsCod { get; set; }

        public InvoicingMethodEnum InvoicingMethod { get; set; }
    }
}
