using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverMessageEmailSenderBackgroundJobArgs
    {
        public int TenantId { get; set; }
        public UserIdentifier RequestorUser { get; set; }
        public int DriverId { get; set; }
        public string DriverFullName { get; set; }
        public string EmailAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
