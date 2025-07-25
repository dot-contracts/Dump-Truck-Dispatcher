namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class RequestDto
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public string ServiceAndMethodName { get; set; }
        public int AverageExecutionDuration { get; set; }
        public int NumberOfTransactions { get; set; }
    }
}
