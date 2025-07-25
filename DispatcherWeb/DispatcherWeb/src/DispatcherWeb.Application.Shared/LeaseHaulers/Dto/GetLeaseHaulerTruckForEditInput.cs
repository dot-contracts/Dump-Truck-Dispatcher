using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class GetLeaseHaulerTruckForEditInput : NullableIdNameDto
    {
        public int? LeaseHaulerId { get; set; }
        public int? VehicleCategoryId { get; set; }
    }
}
