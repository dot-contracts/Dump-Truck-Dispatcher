using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.LeaseHaulers
{
    [Table("LeaseHaulerContact")]
    public class LeaseHaulerContact : FullAuditedEntity, IMustHaveTenant
    {
        public const int MaxCellPhoneNumberLength = 15;

        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(40)]
        public string Title { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(120)]
        public string Email { get; set; }

        [Required]
        public int LeaseHaulerId { get; set; }

        public bool IsDispatcher { get; set; }

        public bool AllowPortalAccess { get; set; }

        [StringLength(MaxCellPhoneNumberLength)]
        public string CellPhoneNumber { get; set; }

        public OrderNotifyPreferredFormat NotifyPreferredFormat { get; set; }

        public virtual LeaseHauler LeaseHauler { get; set; }
    }
}
