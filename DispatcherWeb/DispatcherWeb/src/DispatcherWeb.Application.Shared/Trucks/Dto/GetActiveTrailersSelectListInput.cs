using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetActiveTrailersSelectListInput : GetSelectListInput
    {
        public int? Id { get; set; }
        public int? LeaseHaulerId { get; set; }
        public int? VehicleCategoryId { get; set; }
        public BedConstructionEnum? BedConstruction { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
    }
}
