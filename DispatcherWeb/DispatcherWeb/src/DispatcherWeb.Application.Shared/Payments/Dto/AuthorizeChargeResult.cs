using System;

namespace DispatcherWeb.Payments.Dto
{
    public class AuthorizeChargeResult
    {
        public int PaymentId { get; set; }
        public DateTime? AuthorizationDateTime { get; set; }
    }
}
