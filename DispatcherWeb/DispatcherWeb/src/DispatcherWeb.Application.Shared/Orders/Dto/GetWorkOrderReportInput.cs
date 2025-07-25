using System;
using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class GetWorkOrderReportInput
    {
        public int? Id { get; set; }

        public List<int> Ids { get; set; }

        public DateTime? Date { get; set; }

        public int? OfficeId { get; set; }

        public bool HidePrices { get; set; }

        public bool SplitRateColumn { get; set; }

        public bool ShowPaymentStatus { get; set; }

        public bool ShowSpectrumNumber { get; set; }

        public bool ShowOfficeName { get; set; }

        public bool UseActualAmount { get; set; }

        public bool UseReceipts { get; set; }

        public bool ShowDriverNamesOnPrintedOrder { get; set; }

        public bool ShowDeliveryInfo { get; set; }

        public bool IncludeTickets { get; set; }

        public bool DebugLayout { get; set; }
    }
}
