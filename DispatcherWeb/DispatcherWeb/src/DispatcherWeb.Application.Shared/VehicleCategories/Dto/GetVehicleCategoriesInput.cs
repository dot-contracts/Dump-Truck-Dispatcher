using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.VehicleCategories.Dto
{
    public class GetVehicleCategoriesInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public string Name { get; set; }

        public AssetType? AssetType { get; set; }

        public bool? IsPowered { get; set; }

        public int SortOrder { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = nameof(SortOrder);
            }
        }
    }
}
