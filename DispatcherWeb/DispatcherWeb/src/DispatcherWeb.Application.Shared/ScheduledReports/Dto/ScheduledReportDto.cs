namespace DispatcherWeb.ScheduledReports.Dto
{
    public class ScheduledReportDto
    {
        public int Id { get; set; }
        public string ReportName { get; set; }
        public string SendTo { get; set; }
        public string ReportFormat { get; set; }
        public string ScheduleTime { get; set; }
        public string SendOn { get; set; }
    }
}
