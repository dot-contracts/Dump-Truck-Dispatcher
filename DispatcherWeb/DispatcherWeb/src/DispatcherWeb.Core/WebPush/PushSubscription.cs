using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;

namespace DispatcherWeb.WebPush
{
    public class PushSubscription : FullAuditedEntity
    {
        [StringLength(2048)]
        public string Endpoint { get; set; }
        [StringLength(520)]
        public string P256dh { get; set; }//87-88. for 512 bits max 88+2=90 chars? 512 + 2 chars for 3072 bit
        [StringLength(520)]
        public string Auth { get; set; }//22-24

        public virtual ICollection<DriverPushSubscription> DriverPushSubscriptions { get; set; }

        public bool Equals(PushSubscription other)
        {
            return other != null
                && other.Endpoint == Endpoint
                && other.P256dh == P256dh
                && other.Auth == Auth;
        }

        public static PushSubscription FromDto(PushSubscriptionDto dto)
        {
            return new PushSubscription
            {
                Endpoint = dto.Endpoint,
                P256dh = dto.Keys.P256dh,
                Auth = dto.Keys.Auth,
            };
        }

        public PushSubscriptionDto ToDto()
        {
            return new PushSubscriptionDto
            {
                Endpoint = Endpoint,
                Keys = new PushSubscriptionDto.PushSubscriptionKeys
                {
                    P256dh = P256dh,
                    Auth = Auth,
                },
            };
        }
    }
}
