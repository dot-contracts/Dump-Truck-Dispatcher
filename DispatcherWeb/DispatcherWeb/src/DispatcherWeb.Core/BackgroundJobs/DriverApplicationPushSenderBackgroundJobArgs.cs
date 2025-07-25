using Abp;
using DispatcherWeb.SyncRequests;

namespace DispatcherWeb.BackgroundJobs
{

    public class DriverApplicationPushSenderBackgroundJobArgs : IHaveSyncRequestString
    {
        public UserIdentifier RequestorUser { get; set; }

        public string SyncRequestString { get; set; }
    }
}
