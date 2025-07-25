using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class GetLeaseHaulersSelectListInput : GetSelectListInput
    {
        public bool? HasHaulingCompanyTenantId { get; set; }

        public bool IncludeInactive { get; set; }
    }
}
