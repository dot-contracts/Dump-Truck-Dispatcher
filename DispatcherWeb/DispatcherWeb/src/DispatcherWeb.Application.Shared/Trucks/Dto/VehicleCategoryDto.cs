using DispatcherWeb.VehicleCategories.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class VehicleCategoryDto
    {
        public VehicleCategoryDto()
        {
        }

        public VehicleCategoryDto(VehicleCategoryCacheItem vehicleCategory)
        {
            Id = vehicleCategory.Id;
            Name = vehicleCategory.Name;
            AssetType = vehicleCategory.AssetType;
            IsPowered = vehicleCategory.IsPowered;
            SortOrder = vehicleCategory.SortOrder;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public AssetType AssetType { get; set; }
        public bool IsPowered { get; set; }
        public int SortOrder { get; set; }
    }
}
