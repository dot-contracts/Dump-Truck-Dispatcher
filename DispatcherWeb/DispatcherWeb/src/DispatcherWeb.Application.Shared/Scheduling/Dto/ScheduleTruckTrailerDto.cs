using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.VehicleCategories.Dto;
using VehicleCategoryDto = DispatcherWeb.Trucks.Dto.VehicleCategoryDto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleTruckTrailerDto
    {
        public ScheduleTruckTrailerDto()
        {
        }

        public ScheduleTruckTrailerDto(TruckListCacheItem truck, VehicleCategoryCacheItem vehicleCategory)
        {
            Id = truck.Id;
            TruckCode = truck.TruckCode;
            VehicleCategory = new VehicleCategoryDto(vehicleCategory);
            Year = truck.Year;
            Make = truck.Make;
            Model = truck.Model;
            BedConstruction = truck.BedConstruction;
        }

        public int Id { get; set; }
        public string TruckCode { get; set; }
        public VehicleCategoryDto VehicleCategory { get; set; }
        public int? Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public BedConstructionEnum? BedConstruction { get; set; }
        public string BedConstructionFormatted => BedConstruction.GetDisplayName();
    }
}
