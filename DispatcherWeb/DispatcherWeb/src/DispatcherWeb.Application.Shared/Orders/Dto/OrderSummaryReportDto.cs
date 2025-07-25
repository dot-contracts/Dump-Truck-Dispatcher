using System;
using System.Collections.Generic;
using System.Globalization;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderSummaryReportDto
    {
        public bool HidePrices { get; set; }
        public DateTime Date { get; set; }
        public List<OrderSummaryReportItemDto> Items { get; set; }
        public bool UseShifts { get; set; }
        public CultureInfo CurrencyCulture { get; set; }
    }
}
