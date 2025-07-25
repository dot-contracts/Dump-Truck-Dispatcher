namespace DispatcherWeb.Infrastructure.Sms.Dto
{
    public class SmsSendInput
    {
        public string Body { get; set; }
        public string ContactName { get; set; }
        public string ToPhoneNumber { get; set; }
        public bool TrackStatus { get; set; }
        public bool InsertEntity { get; set; } = true;

        /// <summary>
        /// Set this to true if you want to force SmsSender to use "From Phone Number" specified for the tenant instead of falling back to phone number specified in host settings
        /// </summary>
        public bool DisallowFallbackToHostFromPhoneNumber { get; set; }
    }
}
