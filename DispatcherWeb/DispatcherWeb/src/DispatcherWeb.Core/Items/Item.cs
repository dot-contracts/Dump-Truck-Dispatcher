using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.HaulingCategories;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.Items
{
    [Table("Item")]
    public class Item : FullAuditedEntity, IMustHaveTenant
    {
        public Item()
        {
            OfficeItemPrices = new HashSet<OfficeItemPrice>();
        }

        public int TenantId { get; set; }

        [Required(ErrorMessage = "Name is a required field")]
        [StringLength(EntityStringFieldLengths.Item.Name)]
        public string Name { get; set; }

        [StringLength(EntityStringFieldLengths.Item.Description)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ItemType? Type { get; set; }

        public bool IsTaxable { get; set; }

        [StringLength(EntityStringFieldLengths.Item.IncomeAccount)]
        public string IncomeAccount { get; set; }

        [StringLength(EntityStringFieldLengths.Item.ExpenseAccount)]
        public string ExpenseAccount { get; set; }

        public bool UseZoneBasedRates { get; set; }

        public bool IsInQuickBooks { get; set; }

        public int? MergedToId { get; set; }

        public virtual ICollection<OfficeItemPrice> OfficeItemPrices { get; set; }

        public virtual ICollection<QuoteLine> QuoteFreightItems { get; set; }

        public virtual ICollection<QuoteLine> QuoteMaterialItems { get; set; }

        public virtual ICollection<OrderLine> OrderLineFreightItems { get; set; }

        public virtual ICollection<OrderLine> OrderLineMaterialItems { get; set; }

        public virtual ICollection<ReceiptLine> ReceiptLineFreightItems { get; set; }

        public virtual ICollection<ReceiptLine> ReceiptLineMaterialItems { get; set; }

        public virtual ICollection<Ticket> FreightTickets { get; set; }

        public virtual ICollection<Ticket> MaterialTickets { get; set; }

        public virtual ICollection<ProductLocation> ProductLocations { get; set; }

        public virtual ICollection<HaulingCategory> HaulingCategories { get; set; }

    }
}
