using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers.Dto
{
    public class GetActiveCustomersSelectListInput : GetSelectListInput
    {
        public bool IncludeInactiveWithInvoices { get; set; }
    }
}
