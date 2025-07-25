using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class GetLeaseHaulersInput : PagedAndSortedInputDto, IShouldNormalize, IGetLeaseHaulerListFilter
    {
        public string Name { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public FilterActiveStatus? Status { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
