using System;
using System.Linq;

namespace DispatcherWeb.HostEmails.Dto
{
    public class HostEmailReceiverDto
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
        public string UserFullName { get; set; }
        public string EmailAddress { get; set; }
        public EmailDeliveryStatus? EmailDeliveryStatus { get; set; }
        public string EmailDeliveryStatusFormatted => GetFormattedEmailDeliveryStatus();

        private string GetFormattedEmailDeliveryStatus()
        {
            if (EmailDeliveryStatus == null)
            {
                return HostEmailProcessedAtDateTime.HasValue ? "Failed" : "Not processed";
            }

            if (EmailDeliveryStatuses.Sent.Contains(EmailDeliveryStatus.Value))
            {
                return $"Sent ({EmailDeliveryStatus})";
            }

            if (EmailDeliveryStatuses.Failed.Contains(EmailDeliveryStatus.Value))
            {
                return $"Failed ({EmailDeliveryStatus})";
            }

            if (EmailDeliveryStatus == DispatcherWeb.EmailDeliveryStatus.Delivered)
            {
                return "Delivered";
            }

            if (EmailDeliveryStatus == DispatcherWeb.EmailDeliveryStatus.Opened)
            {
                return "Opened";
            }

            return "Unknown";
        }

        public DateTime? HostEmailProcessedAtDateTime { get; set; }
    }
}
