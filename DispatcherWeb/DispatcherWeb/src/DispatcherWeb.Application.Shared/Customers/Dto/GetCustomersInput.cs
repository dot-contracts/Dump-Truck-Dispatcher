using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomersInput : PagedAndSortedInputDto, IShouldNormalize, IGetCustomerListFilter
    {
        public string Name { get; set; }

        public FilterActiveStatus Status { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
