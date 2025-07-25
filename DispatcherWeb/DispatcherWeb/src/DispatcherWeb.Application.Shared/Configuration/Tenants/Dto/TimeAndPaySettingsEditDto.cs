namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class TimeAndPaySettingsEditDto
    {
        public int TimeTrackingDefaultTimeClassificationId { get; set; }
        public string TimeTrackingDefaultTimeClassificationName { get; set; }
        public bool BasePayOnHourlyJobRate { get; set; }
        public bool UseDriverSpecificHourlyJobRate { get; set; }
        public bool AllowProductionPay { get; set; }
        public bool DefaultToProductionPay { get; set; }
        public bool PreventProductionPayOnHourlyJobs { get; set; }
        public bool AllowDriverPayRateDifferentFromFreightRate { get; set; }
        public bool AllowLoadBasedRates { get; set; }
        public bool ShowFreightRateOnDriverPayStatementReport { get; set; }
        public bool ShowDriverPayRateOnDriverPayStatementReport { get; set; }
        public bool ShowQuantityOnDriverPayStatementReport { get; set; }
        public DriverIsPaidForLoadBasedOnEnum DriverIsPaidForLoadBasedOn { get; set; }
        public PayStatementReportOrientation PayStatementReportOrientation { get; set; }
    }
}
