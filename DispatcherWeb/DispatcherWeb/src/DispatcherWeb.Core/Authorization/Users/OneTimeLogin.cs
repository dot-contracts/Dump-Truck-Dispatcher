using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Authorization.Users
{
    [Table("OneTimeLogin")]
    public class OneTimeLogin : FullAuditedEntity<Guid>
    {
        public long UserId { get; set; }

        public bool IsExpired { get; set; }

        public DateTime? ExpiryTime { get; set; }

        public virtual User User { get; set; }
    }
}
