using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class GetLeaseHaulerTrucksSelectListInput : GetSelectListInput
    {
        public int LeaseHaulerId { get; set; }
        public AssetType? AssetType { get; set; }
    }
}
