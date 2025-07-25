using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomerContactForEditInput : NullableIdNameDto
    {
        public int? CustomerId { get; set; }
    }
}
