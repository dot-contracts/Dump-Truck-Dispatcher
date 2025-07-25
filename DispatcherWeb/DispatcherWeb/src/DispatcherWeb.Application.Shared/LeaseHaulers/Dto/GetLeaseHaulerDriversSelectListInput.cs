using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class GetLeaseHaulerDriversSelectListInput : GetSelectListInput
    {
        public int? LeaseHaulerId { get; set; }
    }
}
