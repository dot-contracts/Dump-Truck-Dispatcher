namespace DispatcherWeb.Orders
{
    public static class OrderItemFormatter
    {
        public static string GetItemWithQuantityFormatted(IOrderLineItemWithQuantity orderLine)
        {
            var hasMaterial = orderLine.MaterialQuantity > 0;

            var itemNamesSeparator = orderLine.FreightItemName != null && orderLine.MaterialItemName != null ? " of " : "";
            var itemName = $"{orderLine.FreightItemName}{itemNamesSeparator}{orderLine.MaterialItemName} - ";
            var freightQuantity = orderLine.FreightQuantity != null ? $"{orderLine.FreightQuantity.Value:0.##} " : "";
            var quantityAndUom = hasMaterial ? $"{orderLine.MaterialQuantity.Value:0.##} {orderLine.MaterialUomName}" : $"{freightQuantity}{orderLine.FreightUomName}";

            return $"{itemName}{quantityAndUom}";
        }
    }
}
