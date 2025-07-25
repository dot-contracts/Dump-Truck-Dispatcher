using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerContactEditDto
    {
        public int? Id { get; set; }

        [Required]
        public int LeaseHaulerId { get; set; }

        [Required(ErrorMessage = "Name is a required field")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(40)]
        public string Title { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(120)]
        public string Email { get; set; }

        public string CellPhoneNumber { get; set; }

        public OrderNotifyPreferredFormat NotifyPreferredFormat { get; set; }

        public bool IsDispatcher { get; set; }

        public bool AllowPortalAccess { get; set; }

        public bool SendInviteEmail { get; set; }
    }
}
