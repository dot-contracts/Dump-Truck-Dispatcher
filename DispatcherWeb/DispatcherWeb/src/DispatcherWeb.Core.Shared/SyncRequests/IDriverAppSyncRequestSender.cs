using System.Threading.Tasks;

namespace DispatcherWeb.SyncRequests
{
    public interface IDriverAppSyncRequestSender
    {
        Task SendSyncRequestAsync(SyncRequest syncRequest);
    }
}
