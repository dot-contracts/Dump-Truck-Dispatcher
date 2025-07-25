using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class DriverAppInfo
    {
        public bool IsDriver { get; set; }
        public bool IsAdmin { get; set; }
        public long UserId { get; set; }
        public GetElapsedTimeResult ElapsedTime { get; set; }
        public bool UseShifts { get; set; }
        public bool UseBackgroundSync { get; set; }
        public int HttpRequestTimeout { get; set; }
        public IDictionary<int, string> ShiftNames { get; set; }
        public Guid DriverGuid { get; set; }
        public string DriverName { get; set; }
        public int? DriverLeaseHaulerId { get; set; }
        public bool HideTicketControls { get; set; }
        [Obsolete]
        public bool RequireToEnterTickets { get; set; }
        public bool RequireSignature { get; set; }
        public bool RequireTicketPhoto { get; set; }
        public string TextForSignatureView { get; set; }
        public bool DispatchesLockedToTruck { get; set; }
        public int? DeviceId { get; set; }
        public List<TimeClassificationDto> TimeClassifications { get; set; }
        public int ProductionPayId { get; set; }
        public bool AutoGenerateTicketNumbers { get; set; }
        public bool DisableTicketNumberOnDriverApp { get; set; }
        public bool AllowLoadCountOnHourlyJobs { get; set; }
        public bool AllowEditingTimeOnHourlyJobs { get; set; }
        public bool AllowMultipleDispatchesToBeInProgressAtTheSameTime { get; set; }
        public bool BasePayOnHourlyJobRate { get; set; }
        public bool UseDriverSpecificHourlyJobRate { get; set; }
        public bool HideDriverAppTimeScreen { get; set; }
        public LogLevel LogLevel { get; set; }
        public int LocationTimeout { get; set; }
        public int LocationMaxAge { get; set; }
        public bool EnableLocationHighAccuracy { get; set; }
        public bool SeparateMaterialAndFreightItemsFeature { get; set; }
    }
}
