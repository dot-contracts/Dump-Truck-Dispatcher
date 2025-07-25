using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class UpdateMileageJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }

    }
}
