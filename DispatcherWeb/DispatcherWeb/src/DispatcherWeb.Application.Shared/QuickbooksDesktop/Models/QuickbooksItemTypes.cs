namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public static class QuickbooksItemTypes
    {
        public const string SalesTax = "COMPTAX";
        public const string Discount = "DISC";

        /// <summary>
        /// Group item (groups several invoice items into a single item)
        /// </summary>
        public const string Group = "GRP";

        public const string InventoryPart = "INVENTORY";
        public const string OtherCharge = "OTHC";
        public const string NonInventoryPart = "PART";
        public const string Payment = "PMT";
        public const string Service = "SERV";
        public const string SalesTaxGroup = "STAX";
        public const string Subtotal = "SUBT";
        public const string InventoryAssembly = "ASSEMBLY";

        public static string FromItemType(ItemType? itemType)
        {
            switch (itemType)
            {
                case ItemType.Discount:
                    return Discount;
                case ItemType.InventoryPart:
                    return InventoryPart;
                case ItemType.NonInventoryPart:
                    return NonInventoryPart;
                case ItemType.OtherCharge:
                    return OtherCharge;
                case ItemType.Payment:
                    return Payment;
                case ItemType.SalesTaxItem:
                    return SalesTax;
                case ItemType.Service:
                    return Service;
                default:
                case ItemType.System:
                    return null;
            }
        }
    }
}
