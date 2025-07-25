namespace DispatcherWeb.VehicleCategories.Dto
{
    public class VehicleCategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public AssetType AssetType { get; set; }

        public string AssetTypeName { get; set; }

        public bool IsPowered { get; set; }

        public int SortOrder { get; set; }
    }
}
