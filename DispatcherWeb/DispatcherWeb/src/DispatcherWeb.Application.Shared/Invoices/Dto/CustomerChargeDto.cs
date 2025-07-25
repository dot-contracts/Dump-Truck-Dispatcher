using System;

namespace DispatcherWeb.Invoices.Dto
{
    public class CustomerChargeDto
    {
        public int Id { get; set; }
        public int OrderLineId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime ChargeDate { get; set; }
        public string Description { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public ItemType? ItemType { get; set; }
        public string UnitOfMeasureName { get; set; }
        public decimal? Quantity { get; set; }
        public bool UseMaterialQuantity { get; set; }
        public decimal Rate { get; set; }
        public decimal ChargeAmount { get; set; }
        public string JobNumber { get; set; }
        public string PoNumber { get; set; }
        public decimal? SalesTaxRate { get; set; }
        public int? SalesTaxEntityId { get; set; }
        public string SalesTaxEntityName { get; set; }
        public bool IsTaxable { get; set; }
        public decimal Tax => IsTaxable ? Math.Round(ChargeAmount * (SalesTaxRate ?? 0) / 100, 2) : 0;

        public decimal Subtotal => FreightTotal + MaterialTotal;

        public decimal Total => Subtotal + Tax;

        public decimal FreightRate => IsMaterial ? 0 : Rate;

        public decimal MaterialRate => IsMaterial ? Rate : 0;

        public decimal FreightTotal => IsMaterial ? 0 : ChargeAmount;

        public decimal MaterialTotal => IsMaterial ? ChargeAmount : 0;

        public decimal FuelSurcharge => 0;

        public bool IsMaterial => IsChargeMaterial(ItemType);

        public static bool IsChargeMaterial(ItemType? itemType)
        {
            switch (itemType)
            {
                case DispatcherWeb.ItemType.NonInventoryPart:
                case DispatcherWeb.ItemType.InventoryPart:
                    return true;
                default:
                    return false;
            }
        }
    }
}
