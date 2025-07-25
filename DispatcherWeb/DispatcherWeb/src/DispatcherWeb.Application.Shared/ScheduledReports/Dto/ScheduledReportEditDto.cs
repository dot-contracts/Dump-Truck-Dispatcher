using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.ScheduledReports.Dto
{
    public class ScheduledReportEditDto
    {
        public int Id { get; set; }
        public ReportType ReportType { get; set; }

        [StringLength(2000)]
        public string SendTo { get; set; }

        public ReportFormat ReportFormat { get; set; }

        public string ScheduleTime { get; set; }

        public int[] SendOnDaysOfWeek { get; set; }

    }
}
