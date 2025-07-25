using System;

namespace DispatcherWeb.Dashboard.Dto
{
    public class GetTruckUtilizationDataInput
    {
        public TruckUtilizationDatePeriod DatePeriod { get; set; }
        public DateTime PeriodBegin { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
