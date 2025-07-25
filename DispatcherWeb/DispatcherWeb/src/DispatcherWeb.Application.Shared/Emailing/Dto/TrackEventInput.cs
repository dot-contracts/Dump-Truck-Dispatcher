using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DispatcherWeb.Emailing.Dto
{
    public class TrackEventInput
    {
        [JsonProperty("trackableEmailId")]
        public Guid? TrackableEmailId { get; set; }

        public int? TrackableEmailTenantId { get; set; }

        public string TrackableEmailCallbackUrl { get; set; }

        public string Event { get; set; } //One of: bounce, deferred, delivered, dropped, processed

        [JsonProperty("email")]
        public string Email { get; set; } //Email address of the intended recipient

        public long Timestamp { get; set; } //UNIX timestamp

        [JsonProperty("sg_event_id")]
        public string SendGridEventId { get; set; }

        public string Reason { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> OtherFields;

        public EmailDeliveryStatus DeliveryStatus
        {
            get
            {
                switch (Event)
                {
                    case "processed": return EmailDeliveryStatus.Processed;
                    case "dropped": return EmailDeliveryStatus.Dropped;
                    case "deferred": return EmailDeliveryStatus.Deferred;
                    case "delivered": return EmailDeliveryStatus.Delivered;
                    case "bounce": return EmailDeliveryStatus.Bounced;
                    case "open": return EmailDeliveryStatus.Opened;
                    default: return EmailDeliveryStatus.NotProcessed;
                }
            }
        }

    }
}
