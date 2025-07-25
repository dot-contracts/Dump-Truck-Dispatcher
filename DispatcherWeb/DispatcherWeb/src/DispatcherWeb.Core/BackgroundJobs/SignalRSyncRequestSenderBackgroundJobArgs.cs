using Abp;
using DispatcherWeb.SyncRequests;

namespace DispatcherWeb.BackgroundJobs
{
    public class SignalRSyncRequestSenderBackgroundJobArgs : IHaveSyncRequestString
    {
        public UserIdentifier RequestorUser { get; set; }

        public string SyncRequestString { get; set; }
    }
}
