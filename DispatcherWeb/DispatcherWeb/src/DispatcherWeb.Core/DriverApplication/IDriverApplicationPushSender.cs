using System.Threading.Tasks;
using DispatcherWeb.BackgroundJobs;

namespace DispatcherWeb.DriverApplication
{
    public interface IDriverApplicationPushSender
    {
        Task SendPushMessageToDriversImmediately(DriverApplicationPushSenderBackgroundJobArgs input);
    }
}
