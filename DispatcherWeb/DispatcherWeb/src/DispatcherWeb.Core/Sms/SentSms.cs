using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Sms
{
    [Table("SentSms")]
    public class SentSms : CreationAuditedEntity, IMayHaveTenant
    {
        [StringLength(EntityStringFieldLengths.TrackableSms.Sid)]
        public string Sid { get; set; }

        public int? TenantId { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string FromSmsNumber { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string ToSmsNumber { get; set; }

        public SmsStatus Status { get; set; }
    }
}
