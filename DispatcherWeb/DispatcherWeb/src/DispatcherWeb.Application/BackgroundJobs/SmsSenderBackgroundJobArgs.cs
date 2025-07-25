using System.Collections.Generic;
using Abp;
using DispatcherWeb.Infrastructure.Sms.Dto;

namespace DispatcherWeb.BackgroundJobs
{
    public class SmsSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public List<SmsSendInput> SmsInputs { get; set; }
    }
}
