using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetTruckForEditInput : NullableIdNameDto
    {
        public int? VehicleCategoryId { get; set; }
    }
}
