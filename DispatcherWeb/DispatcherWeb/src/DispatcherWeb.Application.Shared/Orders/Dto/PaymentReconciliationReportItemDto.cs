using System;
using System.Globalization;

namespace DispatcherWeb.Orders.Dto
{
    public class PaymentReconciliationReportItemDto
    {
        public int PaymentId { get; set; }
        public int? OrderId { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string CustomerName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal? CaptureAmount { get; set; }
        public string TransactionId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string CardLast4 { get; set; }
        public string CardType { get; set; }
        public decimal? AuthorizationAmount { get; set; }
        public string BatchSummaryId { get; set; }

        public string TimeZone { get; set; }
        public CultureInfo CurrencyCulture { get; set; }
    }
}
