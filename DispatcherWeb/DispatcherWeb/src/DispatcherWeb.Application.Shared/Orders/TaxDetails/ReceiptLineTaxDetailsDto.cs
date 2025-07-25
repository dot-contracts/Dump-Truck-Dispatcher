namespace DispatcherWeb.Orders.TaxDetails
{
    public class ReceiptLineTaxDetailsDto : IOrderLineTaxDetails
    {
        public bool? IsTaxable { get; set; }

        public bool? IsMaterialTaxable { get; set; }

        public bool? IsFreightTaxable { get; set; }
        public decimal MaterialAmount { get; set; }
        public decimal FreightAmount { get; set; }

        decimal IOrderLineTaxDetails.MaterialPrice => MaterialAmount;

        decimal IOrderLineTaxDetails.FreightPrice => FreightAmount;
    }
}
