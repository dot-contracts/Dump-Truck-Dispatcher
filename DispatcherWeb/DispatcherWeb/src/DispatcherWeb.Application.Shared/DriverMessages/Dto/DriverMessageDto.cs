using System;

namespace DispatcherWeb.DriverMessages.Dto
{
    public class DriverMessageDto
    {
        public int Id { get; set; }
        public DateTime TimeSent { get; set; }
        public string Driver { get; set; }
        public string SentBy { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public string Status => MessageType == DriverMessageType.Email
            ? EmailStatus.GetDisplayName()
            : SmsStatus.GetDisplayName();

        public EmailDeliveryStatus EmailStatus { get; set; }
        public SmsStatus SmsStatus { get; set; }
        public DriverMessageType MessageType { get; set; }
    }
}
