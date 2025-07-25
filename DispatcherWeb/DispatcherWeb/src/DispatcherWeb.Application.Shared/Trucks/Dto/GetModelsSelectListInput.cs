using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetModelsSelectListInput : GetSelectListInput
    {
        public int? VehicleCategoryId { get; set; }
        public string Make { get; set; }
    }
}
