using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class FulcrumDispatchDtdTicketJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }

        public int DispatchId { get; set; }

        public FulcrumDtdTicketAction Action { get; set; }

    }
}
