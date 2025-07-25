using System;

namespace DispatcherWeb.Invoices.Dto
{
    public class CalculateDueDateInput
    {
        public DateTime IssueDate { get; set; }
        public BillingTermsEnum? Terms { get; set; }
    }
}
