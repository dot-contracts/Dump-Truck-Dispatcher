using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class ReceiptReportItemDto : IOrderLineTaxDetails
    {
        public bool? IsTaxable { get; set; }

        public bool? IsMaterialTaxable { get; set; }

        public bool? IsFreightTaxable { get; set; }
        public decimal? ActualMaterialQuantity { get; set; }
        public decimal? ActualFreightQuantity { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        public decimal ReceiptLineMaterialPrice { get; set; }
        public decimal ReceiptLineFreightPrice { get; set; }
        public decimal MaterialPrice => IsOrderLineMaterialPriceOverridden
            ? OrderLineMaterialPrice
            : ReceiptLineMaterialPrice;
        public decimal FreightPrice => IsOrderLineFreightPriceOverridden
            ? OrderLineFreightPrice
            : ReceiptLineFreightPrice;
        public decimal OrderLineMaterialPrice { get; set; }
        public decimal OrderLineFreightPrice { get; set; }
        public bool IsOrderLineMaterialPriceOverridden { get; set; }
        public bool IsOrderLineFreightPriceOverridden { get; set; }
    }
}
