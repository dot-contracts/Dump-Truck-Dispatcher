using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.VehicleCategories.Dto
{
    public class VehicleCategoryEditDto
    {
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }

        public AssetType AssetType { get; set; }

        public bool IsPowered { get; set; }

        [Required]
        public int? SortOrder { get; set; }
    }
}
