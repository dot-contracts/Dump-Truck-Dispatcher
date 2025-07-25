using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetVehicleCategoriesSelectListInput : GetSelectListInput
    {
        public bool? IsPowered { get; set; }
        public bool? IsInUse { get; set; }
        public AssetType? AssetType { get; set; }
    }
}
