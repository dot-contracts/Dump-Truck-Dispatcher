using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.HostEmails.Dto
{
    public class HostEmailDto
    {
        public const int MaxLengthOfBodyAndSubject = 50;

        public int Id { get; set; }
        public DateTime SentAtDateTime { get; set; }
        public DateTime? ProcessedAtDateTime { get; set; }
        public string SentByUserFullName { get; set; }
        public HostEmailType Type { get; set; }
        public string TypeFormatted => Type.GetDisplayName();

        public string Subject { get; set; }
        public string Body { get; set; }
        public int? SentEmailCount => Receivers.Count(r => r.DeliveryStatus.HasValue);
        public int? DeliveredEmailCount => GetReceiverCount(EmailDeliveryStatus.Delivered, EmailDeliveryStatus.Opened);
        public int? OpenedEmailCount => GetReceiverCount(EmailDeliveryStatus.Opened);
        public int? FailedEmailCount => GetReceiverCount(EmailDeliveryStatuses.Failed)
            + (ProcessedAtDateTime.HasValue ? Receivers.Count(r => r.DeliveryStatus == null) : 0);

        public List<Receiver> Receivers { get; set; }

        private int? GetReceiverCount(params EmailDeliveryStatus[] deliveryStatuses)
        {
            return Receivers.Count(r => r.DeliveryStatus.HasValue && deliveryStatuses.Contains(r.DeliveryStatus.Value));
        }

        public class Receiver
        {
            public EmailDeliveryStatus? DeliveryStatus { get; set; }
        }
    }
}
