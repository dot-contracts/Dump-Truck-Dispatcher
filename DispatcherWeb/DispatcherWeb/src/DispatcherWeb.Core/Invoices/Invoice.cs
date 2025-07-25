using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Customers;
using DispatcherWeb.Emailing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices;
using DispatcherWeb.TaxRates;

namespace DispatcherWeb.Invoices
{
    [Table("Invoice")]
    public class Invoice : FullAuditedEntity, IMustHaveTenant
    {
        public Invoice()
        {
            InvoiceLines = new HashSet<InvoiceLine>();
        }

        public int TenantId { get; set; }

        public int OfficeId { get; set; }

        public virtual Office Office { get; set; }

        public int? BatchId { get; set; }

        public virtual InvoiceBatch Batch { get; set; }

        public int? UploadBatchId { get; set; }

        public virtual InvoiceUploadBatch UploadBatch { get; set; }

        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        [StringLength(EntityStringFieldLengths.General.Email)]
        public string EmailAddress { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.FullAddress)]
        public string BillingAddress { get; set; }

        public BillingTermsEnum? Terms { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Tax { get; set; }

        public InvoiceStatus Status { get; set; }

        public DateTime? QuickbooksExportDateTime { get; set; }

        [StringLength(EntityStringFieldLengths.Invoice.QuickbooksInvoiceId)]
        public string QuickbooksInvoiceId { get; set; }

        [StringLength(EntityStringFieldLengths.Invoice.Message)]
        public string Message { get; set; }

        public decimal TaxRate { get; set; }

        [StringLength(EntityStringFieldLengths.Invoice.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Invoice.PoNumber)]
        public string PoNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Invoice.Description)]
        public string Description { get; set; }

        public ShowFuelSurchargeOnInvoiceEnum ShowFuelSurchargeOnInvoice { get; set; }

        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; }

        public virtual ICollection<InvoiceEmail> InvoiceEmails { get; set; }

        public int? SalesTaxEntityId { get; set; }

        public virtual TaxRate SalesTaxEntity { get; set; }
    }
}
