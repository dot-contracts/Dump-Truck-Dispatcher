using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.DriverApp.Settings.Dto
{
    public class SettingsDto
    {
        public int HttpRequestTimeout { get; set; }
        public bool HideTicketControls { get; set; }
        [Obsolete]
        public bool RequireToEnterTickets { get; set; }
        public bool RequireSignature { get; set; }
        public bool RequireTicketPhoto { get; set; }
        public string TextForSignatureView { get; set; }
        public bool IsUserAdmin { get; set; }
        public bool IsUserDriver { get; set; }
        public bool IsUserLeaseHaulerDriver { get; set; }
        public int? DriverId { get; set; }
        public string UserName { get; set; }
        public DriverAppImageResolutionEnum DriverAppImageResolution { get; set; }
        public int ProductionPayId { get; set; }
        public FeaturesDto Features { get; set; }
        public PermissionsDto Permissions { get; set; }
        public bool AllowEditingTimeOnHourlyJobs { get; set; }
        public bool AllowLoadCountOnHourlyJobs { get; set; }
        public bool AutoGenerateTicketNumbers { get; set; }
        public bool DisableTicketNumberOnDriverApp { get; set; }
        public bool AllowMultipleDispatchesToBeInProgressAtTheSameTime { get; set; }
        public bool BasePayOnHourlyJobRate { get; set; }
        public bool UseDriverSpecificHourlyJobRate { get; set; }
        public bool HideDriverAppTimeScreen { get; set; }
        public LogLevel LoggingLevel { get; set; }
        public bool SyncDataOnButtonClicks { get; set; }
        public int PeriodicSyncCheckIntervalSeconds { get; set; }
        public int LocationTimeout { get; set; }
        public int LocationMaxAge { get; set; }
        public bool EnableLocationHighAccuracy { get; set; }
        public bool UseShifts { get; set; }
        public Dictionary<int, string> ShiftNames { get; set; }
        public string MinimumNativeAppVersion { get; set; }
        public string RecommendedNativeAppVersion { get; set; }
        public string GooglePlayUrl { get; set; }
        public string AppleStoreUrl { get; set; }
        public int SignalRHeartbeatInterval { get; set; }
    }
}
