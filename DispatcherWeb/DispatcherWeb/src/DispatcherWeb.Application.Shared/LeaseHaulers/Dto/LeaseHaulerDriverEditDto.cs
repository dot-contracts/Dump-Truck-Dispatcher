using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerDriverEditDto
    {
        public int? Id { get; set; }

        [Required]
        public int LeaseHaulerId { get; set; }

        [Required(ErrorMessage = "First Name is a required field")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is a required field")]
        [StringLength(50)]
        public string LastName { get; set; }

        public string FullName => FirstName + " " + LastName;

        public bool DriverIsActive { get; set; }

        public bool EnableForDriverApplication { get; set; }

        public long? UserId { get; set; }

        [StringLength(256)]
        public string EmailAddress { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string CellPhoneNumber { get; set; }

        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }

        public bool SetRandomPassword { get; set; }

        public string Password { get; set; }

        public string PasswordRepeat { get; set; }

        public bool ShouldChangePasswordOnNextLogin { get; set; }

        public bool SendActivationEmail { get; set; }

        public int? HaulingCompanyDriverId { get; set; }
    }
}
