using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Invoices;
using DispatcherWeb.Items;
using DispatcherWeb.Orders;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Charges
{
    [Table("Charge")]
    public class Charge : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        public int ItemId { get; set; }

        public virtual Item Item { get; set; }

        [Required]
        public int UnitOfMeasureId { get; set; }

        public virtual UnitOfMeasure UnitOfMeasure { get; set; }

        [StringLength(EntityStringFieldLengths.Charge.Description)]
        public string Description { get; set; }

        public decimal Rate { get; set; }

        public decimal? Quantity { get; set; }

        public bool UseMaterialQuantity { get; set; }

        public decimal ChargeAmount { get; set; }

        [Required]
        public int OrderLineId { get; set; }

        public virtual OrderLine OrderLine { get; set; }

        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; }

        public virtual ICollection<ReceiptLine> ReceiptLines { get; set; }

        public bool IsBilled { get; set; }

        public DateTime ChargeDate { get; set; }
    }
}
