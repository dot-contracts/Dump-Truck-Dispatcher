using System;
using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class ReceiptReportDto : ReceiptReportDto<ReceiptReportItemDto>
    {
    }

    public class ReceiptReportDto<T> /*: IOrderTaxDetails*/ where T : ReceiptReportItemDto
    {
        public int ReceiptId { get; set; }
        public int OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string CustomerName { get; set; }
        public decimal SalesTaxRate { get; set; }
        public List<T> Items { get; set; }
        public decimal FreightTotal { get; set; }
        public decimal MaterialTotal { get; set; }
        public decimal SalesTax { get; set; }
        public decimal CODTotal { get; set; }
    }
}
