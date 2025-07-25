using System.Collections.Generic;
using Abp;

namespace DispatcherWeb.BackgroundJobs.Dto
{
    public class TicketPhotoDownloadJobArgs
    {
        public List<int> TicketIds { get; set; }

        public UserIdentifier RequestorUser { get; set; }

        public string SuccessMessage { get; set; }

        public string FailedMessage { get; set; }

        public string FileName { get; set; }
    }
}
