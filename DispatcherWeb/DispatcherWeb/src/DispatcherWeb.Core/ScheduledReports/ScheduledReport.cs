using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.ScheduledReports
{
    [Table("ScheduledReport")]
    public class ScheduledReport : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public ReportType ReportType { get; set; }

        [StringLength(EntityStringFieldLengths.ScheduledReport.SendTo)]
        public string SendTo { get; set; }

        public ReportFormat ReportFormat { get; set; }

        public TimeSpan ScheduleTime { get; set; }

        public DayOfWeekBitFlag SendOnDaysOfWeek { get; set; }
    }
}
