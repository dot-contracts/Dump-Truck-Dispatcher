using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomerContactsInput : SortedInputDto, IShouldNormalize
    {
        public int CustomerId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
