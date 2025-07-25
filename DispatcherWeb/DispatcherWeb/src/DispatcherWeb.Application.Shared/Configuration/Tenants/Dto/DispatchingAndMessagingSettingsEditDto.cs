using System;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Configuration.Tenants.Dto
{
    public class DispatchingAndMessagingSettingsEditDto
    {
        public DispatchVia DispatchVia { get; set; }

        public bool AllowSmsMessages { get; set; }

        public SendSmsOnDispatchingEnum SendSmsOnDispatching { get; set; }

        public string SmsPhoneNumber { get; set; }

        public string DriverDispatchSms { get; set; }

        public string DriverStartTime { get; set; }

        public bool HideTicketControlsInDriverApp { get; set; }

        public bool RequireSignature { get; set; }

        public bool RequireTicketPhoto { get; set; }

        public string TextForSignatureView { get; set; }

        public bool DispatchesLockedToTruck { get; set; }

        public DateTime DefaultStartTime { get; set; }

        public bool ShowTrailersOnSchedule { get; set; }

        public bool ShowStaggerTimes { get; set; }

        public bool ValidateUtilization { get; set; }

        public bool AllowSchedulingTrucksWithoutDrivers { get; set; }

        public bool AllowCounterSalesForTenant { get; set; }

        public bool AutoGenerateTicketNumbers { get; set; }

        public bool DisableTicketNumberOnDriverApp { get; set; }

        public bool AllowLoadCountOnHourlyJobs { get; set; }

        public bool AllowEditingTimeOnHourlyJobs { get; set; }

        public bool AllowMultipleDispatchesToBeInProgressAtTheSameTime { get; set; }

        public bool HideDriverAppTimeScreen { get; set; }

        public LogLevel LoggingLevel { get; set; }

        public bool SyncDataOnButtonClicks { get; set; }

        public RequiredTicketEntryEnum RequiredTicketEntry { get; set; }

    }
}
