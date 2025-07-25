using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Payments.Dto
{
    public class AuthorizeChargeDto
    {
        public string CreditCardToken { get; set; }
        public string NewCreditCardTempToken { get; set; }
        public decimal AuthorizationAmount { get; set; }
        public string StreetAddress { get; set; }

        [Required]
        public string ZipCode { get; set; }

        public string PaymentDescription { get; set; }
    }
}
