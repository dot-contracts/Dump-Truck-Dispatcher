using System;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.MultiTenancy.Dto
{
    public class GetTenantsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public string Filter { get; set; }
        public DateTime? SubscriptionEndDateStart { get; set; }
        public DateTime? SubscriptionEndDateEnd { get; set; }
        public DateTime? CreationDateStart { get; set; }
        public DateTime? CreationDateEnd { get; set; }
        public int? EditionId { get; set; }
        public bool EditionIdSpecified { get; set; }
        public FilterActiveStatus Status { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(Sorting))
            {
                Sorting = "TenancyName";
            }
        }
    }
}

