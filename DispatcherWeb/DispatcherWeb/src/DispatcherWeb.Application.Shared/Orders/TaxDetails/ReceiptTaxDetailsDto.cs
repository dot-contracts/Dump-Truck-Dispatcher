using System.Collections.Generic;

namespace DispatcherWeb.Orders.TaxDetails
{
    public class ReceiptTaxDetailsDto : IOrderTaxDetails
    {
        public ReceiptTaxDetailsDto()
        {
        }

        public ReceiptTaxDetailsDto(IOrderTaxDetails source)
        {
            Id = source.Id;
            SalesTaxRate = source.SalesTaxRate;
            SalesTax = source.SalesTax;
            CODTotal = source.CODTotal;
            FreightTotal = source.FreightTotal;
            MaterialTotal = source.MaterialTotal;
        }

        public int Id { get; set; }
        public decimal SalesTaxRate { get; set; }
        public decimal SalesTax { get; set; }
        public decimal CODTotal { get; set; }
        public decimal FreightTotal { get; set; }
        public decimal MaterialTotal { get; set; }
        public List<ReceiptLineTaxDetailsDto> ReceiptLines { get; set; }
    }
}
