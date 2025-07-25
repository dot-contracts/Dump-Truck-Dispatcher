using System;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.MultiTenancy.HostDashboard.Dto
{
    public class GetRequestsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "NumberOfTransactions desc";
            }
        }
    }
}
