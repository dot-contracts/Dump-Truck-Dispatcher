using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulerPerformance.Dto
{
    public class GetLeaseHaulerPerformancesInput : SortedInputDto, IShouldNormalize
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "LeaseHaulerName";
            }
        }
    }
}
