using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Payments.Dto
{
    public class CaptureAuthorizationDto
    {
        public int PaymentId { get; set; }

        [Required]
        public decimal? ActualAmount { get; set; }
    }
}
