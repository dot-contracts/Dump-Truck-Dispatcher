using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class FulcrumSyncJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public FulcrumEntity Entity { get; set; }

    }
}
