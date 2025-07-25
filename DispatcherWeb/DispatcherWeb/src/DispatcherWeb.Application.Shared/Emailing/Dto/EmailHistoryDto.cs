using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.Emailing.Dto
{
    public class EmailHistoryDto
    {
        public Guid EmailId { get; set; }
        public string EmailSubject { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverEmail { get; set; }
        public EmailReceiverKind ReceiverKind { get; set; }

        public string ReceiverKindFormatted => ReceiverKind.GetDisplayName();
        public DateTime EmailCreationTime { get; set; }
        public DateTime? EmailDeliveryTime => GetEventTime(EmailDeliveryStatus.Delivered);
        public DateTime? EmailOpenedTime => GetEventTime(EmailDeliveryStatus.Opened);
        public DateTime? EmailFailedTime => GetEventTime(x => x.EmailDeliveryStatus?.IsFailed() == true);
        public string EmailCreatorUserName { get; set; }
        public EmailDeliveryStatus ReceiverDeliveryStatus { get; set; }

        public string ReceiverDeliveryStatusFormatted => ReceiverDeliveryStatus.GetDisplayName();
        public List<EmailHistoryEventDto> Events { get; set; }

        private DateTime? GetEventTime(EmailDeliveryStatus status)
        {
            return Events?.FirstOrDefault(x => x.EmailDeliveryStatus == status)?.CreationTime;
        }
        private DateTime? GetEventTime(Func<EmailHistoryEventDto, bool> predicate)
        {
            return Events?.FirstOrDefault(predicate)?.CreationTime;
        }
    }
}
