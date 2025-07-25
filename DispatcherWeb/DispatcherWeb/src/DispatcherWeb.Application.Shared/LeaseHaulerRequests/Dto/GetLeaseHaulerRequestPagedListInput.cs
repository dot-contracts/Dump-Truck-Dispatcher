using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class GetLeaseHaulerRequestPagedListInput : PagedAndSortedInputDto, IShouldNormalize, IGetLeaseHaulerRequestPagedListInput
    {
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public Shift? Shift { get; set; }
        public int? LeaseHaulerId { get; set; }
        public int? OfficeId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Date";
            }

        }
    }
}
