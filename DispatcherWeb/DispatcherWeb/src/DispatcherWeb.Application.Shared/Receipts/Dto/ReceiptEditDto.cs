using System;
using System.Collections.Generic;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Receipts.Dto
{
    public class ReceiptEditDto : IOfficeIdNameDto
    {
        public int? Id { get; set; }

        public int OrderId { get; set; }

        public DateTime? AuthorizationDateTime { get; set; }

        public DateTime? AuthorizationCaptureDateTime { get; set; }

        public bool IsFreightTotalOverridden { get; set; }

        public bool IsMaterialTotalOverridden { get; set; }

        public DateTime DeliveryDate { get; set; }
        public DateTime ReceiptDate { get; set; }
        public Shift? Shift { get; set; }
        public int OfficeId { get; set; }
        public string OfficeName { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? QuoteId { get; set; }
        public string QuoteName { get; set; }
        public string PoNumber { get; set; }
        public decimal SalesTaxRate { get; set; }

        public decimal FreightTotal { get; set; }

        public decimal MaterialTotal { get; set; }

        public decimal SalesTax { get; set; }

        public decimal Total { get; set; }

        public bool IsSingleOffice { get; set; }

        public List<ReceiptLineEditDto> ReceiptLines { get; set; }
    }
}
