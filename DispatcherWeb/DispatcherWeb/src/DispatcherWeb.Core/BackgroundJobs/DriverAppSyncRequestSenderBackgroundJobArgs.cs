using Abp;
using DispatcherWeb.SyncRequests;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverAppSyncRequestSenderBackgroundJobArgs : IHaveSyncRequestString
    {
        public UserIdentifier RequestorUser { get; set; }

        public string SyncRequestString { get; set; }
    }
}
