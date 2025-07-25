using Abp;
using DispatcherWeb.HostEmails.Dto;

namespace DispatcherWeb.BackgroundJobs
{
    public class HostEmailSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public int HostEmailId { get; set; }
        public SendHostEmailInput Input { get; set; }
    }
}
