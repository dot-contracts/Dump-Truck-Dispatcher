using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class GetLeaseHaulerContactsInput : SortedInputDto, IShouldNormalize
    {
        public int LeaseHaulerId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
