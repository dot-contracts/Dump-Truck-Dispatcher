using System.Collections.Generic;
using Abp;

namespace DispatcherWeb.BackgroundJobs
{
    public class EmailSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public List<EmailSenderBackgroundJobArgsEmail> EmailInputs { get; set; }
    }

    public class EmailSenderBackgroundJobArgsEmail
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ToEmailAddress { get; set; }
        public string ContactName { get; set; }
    }
}
