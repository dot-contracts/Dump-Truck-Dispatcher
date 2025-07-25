using System;

namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementReportItemDto
    {
        public virtual PayStatementItemKind ItemKind { get; set; }
        public virtual DateTime? Date { get; set; }
        public int TimeClassificationId { get; set; }
        public string TimeClassificationName { get; set; }
        public bool IsProductionPay { get; set; }
        public decimal? DriverPayRate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public string Item { get; set; }
        public string CustomerName { get; set; }
        public string JobNumber { get; set; }
        public string TicketNumber { get; set; }
        public string UnitOfMeasure { get; set; }
        public string DeliverToName { get; set; }
        public string LoadAtName { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }
    }
}
