using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using DispatcherWeb.DriverApplication;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverApplicationPushSenderBackgroundJob : AsyncBackgroundJob<DriverApplicationPushSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IDriverApplicationPushSender _driverApplicationPushSender;

        public DriverApplicationPushSenderBackgroundJob(
            IDriverApplicationPushSender driverApplicationPushSender
            )
        {
            _driverApplicationPushSender = driverApplicationPushSender;
        }

        //do not add a unit of work attribute to this method, SendPushMessageToDriversImmediately must handle UOW creation manually
        public override async Task ExecuteAsync(DriverApplicationPushSenderBackgroundJobArgs args)
        {
            await _driverApplicationPushSender.SendPushMessageToDriversImmediately(args);
        }
    }
}
