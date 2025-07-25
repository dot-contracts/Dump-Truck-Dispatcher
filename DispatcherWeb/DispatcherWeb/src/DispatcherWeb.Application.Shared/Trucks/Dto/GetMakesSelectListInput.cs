using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetMakesSelectListInput : GetSelectListInput
    {
        public int? VehicleCategoryId { get; set; }
    }
}
