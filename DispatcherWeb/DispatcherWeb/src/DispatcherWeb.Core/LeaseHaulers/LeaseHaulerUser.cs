using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.LeaseHaulers;

[Table("LeaseHaulerUser")]
public class LeaseHaulerUser : FullAuditedEntity, IMustHaveTenant
{
    public int TenantId { get; set; }
    public int LeaseHaulerId { get; set; }
    public long UserId { get; set; }
    public virtual LeaseHauler LeaseHauler { get; set; }
    public virtual User User { get; set; }
}