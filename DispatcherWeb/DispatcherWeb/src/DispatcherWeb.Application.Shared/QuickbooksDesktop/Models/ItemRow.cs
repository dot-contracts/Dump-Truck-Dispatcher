using System.Text;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public class ItemRow : Row
    {
        public override string RowType => RowTypes.Item;

        public static ItemRowHeader HeaderRow = new ItemRowHeader();

        /// <summary>
        /// (Required) The name of the invoice item.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// (Required) Indicates the type of invoice item. If you are creating an import file, use one of these keywords to indicate the item type.
        /// </summary>
        public virtual string ItemType { get; set; }

        /// <summary>
        /// A description of the item as you want it to appear in the Description column on invoices, credit memos, and sales receipts.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// (Inventory part items only) A description of the item as you want it to appear on purchase orders.
        /// </summary>
        public virtual string PurchaseDescription { get; set; }

        /// <summary>
        /// (Required) The name of the income account you use to track sales of the item. The type of this account should be INC.
        /// </summary>
        public virtual string Account { get; set; }

        /// <summary>
        /// (Inventory part items only) The name of the asset account you use to track the value of your inventory. The type of this account should be OASSET.
        /// </summary>
        public virtual string AssetAccount { get; set; }

        /// <summary>
        /// (Inventory part items only) The name of the account you use to track the cost of your sales. The type of this account should be COGS.
        /// </summary>
        public virtual string CogsAccount { get; set; }

        /// <summary>
        /// (All item types except group, payment, and subtotal) The rate or price you charge for the item. If you are creating an import file, add a percent sign (%) if the amount is a percentage.
        /// </summary>
        public virtual string Price { get; set; }

        /// <summary>
        /// (Inventory part items only) The unit cost of the item.
        /// </summary>
        public virtual string Cost { get; set; }

        /// <summary>
        /// (Discount, other charges, part, and service items only) Indicates whether the item is taxable.If you are creating an import file, enter one of these keywords in the TAXABLE field:
        /// Y - Yes.The item is taxable.
        /// N - No. The item is not taxable.
        /// </summary>
        public virtual string Taxable { get; set; }

        /// <summary>
        /// (Payment items only) The payment method customers use(check, Visa, etc.).
        /// </summary>
        public virtual string PaymentMethod { get; set; }

        /// <summary>
        /// (Sales tax items only) The name of the agency to which you pay sales tax.
        /// </summary>
        public virtual string TaxAgency { get; set; } //TAXVEND

        /// <summary>
        /// (Sales tax items only) The name of your tax district.
        /// </summary>
        public virtual string TaxDistrict { get; set; }

        /// <summary>
        /// (Inventory part items only) The name of the vendor from whom you normally purchase the item.
        /// </summary>
        public virtual string PreferredVendor { get; set; }

        /// <summary>
        /// (Inventory part items only) The minimum quantity you want to keep in stock at any given time. When your inventory reaches this level, QuickBooks informs you that it is time to reorder the item.
        /// </summary>
        public virtual string ReorderPoint { get; set; }

        /// <summary>
        /// Adds additional information about the invoice item. These keywords can appear in the EXTRA field:
        /// AUTOSTAX - Identifies a sales tax item as the automatic tax rate you set up for your QuickBooks company.
        /// REXPGROUP - Indicates that the item is a group of reimbursable expenses that you included on the invoice.
        /// REXPSUBTOT - Indicates that the item is the subtotal amount for a group of reimbursable expenses you included on the invoice.
        /// </summary>
        public virtual string Extra { get; set; }

        public override StringBuilder AppendRow(StringBuilder s)
        {
            return AppendTabSeparatedLine(s, new[]
            {
                RowType,
                Name,
                ItemType,
                Description,
                PurchaseDescription,
                Account,
                AssetAccount,
                CogsAccount,
                Price,
                Cost,
                Taxable,
                PaymentMethod,
                TaxAgency,
                TaxDistrict,
                PreferredVendor,
                ReorderPoint,
                Extra,
            });
        }

        public class ItemRowHeader : ItemRow
        {
            public override string RowType => "!" + base.RowType;
            public override string Name => "NAME";
            public override string ItemType => "INVITEMTYPE";
            public override string Description => "DESC";
            public override string PurchaseDescription => "PURCHASEDESC";
            public override string Account => "ACCNT";
            public override string AssetAccount => "ASSETACCNT";
            public override string CogsAccount => "COGSACCNT";
            public override string Price => "PRICE";
            public override string Cost => "COST";
            public override string Taxable => "TAXABLE";
            public override string PaymentMethod => "PAYMETH";
            public override string TaxAgency => "TAXVEND";
            public override string TaxDistrict => "TAXDIST";
            public override string PreferredVendor => "PREFVEND";
            public override string ReorderPoint => "REORDERPOINT";
            public override string Extra => "EXTRA";
        }
    }
}
