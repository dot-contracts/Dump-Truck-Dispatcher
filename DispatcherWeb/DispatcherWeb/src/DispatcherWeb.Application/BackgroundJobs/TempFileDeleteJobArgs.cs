using Abp;

namespace DispatcherWeb.BackgroundJobs.Dto
{
    public class TempFileDeleteJobArgs
    {
        public int TempFileId { get; set; }
        public UserIdentifier RequestorUser { get; set; }
    }
}
