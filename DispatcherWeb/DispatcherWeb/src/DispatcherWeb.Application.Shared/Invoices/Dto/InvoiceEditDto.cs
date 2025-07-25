using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Invoices.Dto
{
    public class InvoiceEditDto
    {
        public int? Id { get; set; }

        [Required]
        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        [Required]
        public int? OfficeId { get; set; }

        public string OfficeName { get; set; }

        [StringLength(EntityStringFieldLengths.General.Email)]
        public string EmailAddress { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.FullAddress)]
        public string BillingAddress { get; set; }

        public BillingTermsEnum? Terms { get; set; }

        [Required]
        public DateTime? IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal BalanceDue { get; set; }

        public decimal? TaxRate { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxAmount { get; set; }

        public int? SalesTaxEntityId { get; set; }

        public string SalesTaxEntityName { get; set; }

        public InvoiceStatus Status { get; set; }

        public string StatusName => Status.GetDisplayName();

        public int? UploadBatchId { get; set; }

        public int? BatchId { get; set; }

        public string Message { get; set; }

        public string JobNumber { get; set; }

        public string PoNumber { get; set; }

        public string Description { get; set; }

        public InvoicingMethodEnum? CustomerInvoicingMethod { get; set; }

        public ShowFuelSurchargeOnInvoiceEnum ShowFuelSurchargeOnInvoice { get; set; }

        public int? FuelItemId { get; set; }

        public string FuelItemName { get; set; }

        public bool FuelItemIsTaxable { get; set; }

        public List<InvoiceLineEditDto> InvoiceLines { get; set; }
    }
}
