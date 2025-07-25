using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.OrderPayments.Dto
{
    public class CaptureOrderAuthorizationDto
    {
        public int ReceiptId { get; set; }
        public decimal AuthorizationAmount { get; set; }

        [Required]
        public decimal? ActualAmount { get; set; }
    }
}
