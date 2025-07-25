using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.WebPush
{
    [Table("FcmRegistrationToken")]
    public class FcmRegistrationToken : FullAuditedEntity, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }

        public virtual User User { get; set; }

        [StringLength(EntityStringFieldLengths.FcmRegistrationToken.Token)] //the actual data was at least 164 chars long
        public string Token { get; set; }

        public MobilePlatform MobilePlatform { get; set; }

        public int? Version { get; set; }
    }
}
