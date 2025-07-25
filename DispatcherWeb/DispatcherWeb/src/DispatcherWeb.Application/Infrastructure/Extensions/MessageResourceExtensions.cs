using System;
using System.Diagnostics.CodeAnalysis;
using Twilio.Rest.Api.V2010.Account;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class MessageResourceExtensions
    {
        [SuppressMessage("ReSharper", "EnforceIfStatementBraces")]
        [SuppressMessage("Style", "IDE0011:Add braces", Justification = "Switch is unavailable, so `if` operators are formatted in a similar way instead, for readability.")]
        public static SmsStatus ToSmsStatus(this MessageResource.StatusEnum statusEnum)
        {
            if (statusEnum == MessageResource.StatusEnum.Accepted) return SmsStatus.Accepted;
            if (statusEnum == MessageResource.StatusEnum.Delivered) return SmsStatus.Delivered;
            if (statusEnum == MessageResource.StatusEnum.Failed) return SmsStatus.Failed;
            if (statusEnum == MessageResource.StatusEnum.Queued) return SmsStatus.Queued;
            if (statusEnum == MessageResource.StatusEnum.Received) return SmsStatus.Received;
            if (statusEnum == MessageResource.StatusEnum.Receiving) return SmsStatus.Receiving;
            if (statusEnum == MessageResource.StatusEnum.Sending) return SmsStatus.Sending;
            if (statusEnum == MessageResource.StatusEnum.Sent) return SmsStatus.Sent;
            if (statusEnum == MessageResource.StatusEnum.Undelivered) return SmsStatus.Undelivered;
            throw new ArgumentException();
        }
    }
}
