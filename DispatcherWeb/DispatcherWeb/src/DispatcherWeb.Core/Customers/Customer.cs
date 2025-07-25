using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Invoices;
using DispatcherWeb.Items;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Customers
{
    [Table("Customer")]
    public class Customer : FullAuditedEntity, IMustHaveTenant
    {
        public Customer()
        {
            CustomerContacts = new HashSet<CustomerContact>();
        }

        public int TenantId { get; set; }

        /// <summary>
        /// MaterialCompany's Tenant id. Only set for HaulingCompany Customers which represent a whole MaterialCompany tenant
        /// </summary>
        public int? MaterialCompanyTenantId { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.Customer.Name)]
        public string Name { get; set; }

        [StringLength(EntityStringFieldLengths.Customer.AccountNumber)]
        public string AccountNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Customer.TaxIdNumber)]
        public string TaxIdNumber { get; set; }

        public bool IsCod { get; set; }

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

        public PreferredBillingDeliveryMethodEnum? PreferredDeliveryMethod { get; set; }

        public BillingTermsEnum? Terms { get; set; }

        [StringLength(EntityStringFieldLengths.Customer.CreditCardToken)]
        public string CreditCardToken { get; set; }

        [StringLength(EntityStringFieldLengths.Customer.CreditCardStreetAddress)]
        public string CreditCardStreetAddress { get; set; }

        [StringLength(EntityStringFieldLengths.Customer.CreditCardZipCode)]
        public string CreditCardZipCode { get; set; }

        public bool IsActive { get; set; }

        public bool IsTaxExempt { get; set; }

        public bool CombineTickets { get; set; }

        [StringLength(EntityStringFieldLengths.Customer.Notes)]
        public string Notes { get; set; }

        public InvoicingMethodEnum InvoicingMethod { get; set; }

        public bool IsInQuickBooks { get; set; }

        public int? MergedToId { get; set; }

        public int? PricingTierId { get; set; }

        public PricingTier PricingTier { get; set; }

        public virtual ICollection<CustomerContact> CustomerContacts { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }

        //public override string ToString()
        //{
        //    return Name;
        //}
    }
}
