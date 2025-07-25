using DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class TelematicsExtensions
    {

        public static Truck ParseToTruck(this TruckUnitDto truckUnitDto)
        {
            var truck = new Truck
            {
                TruckCode = truckUnitDto.Name,
                IsActive = truckUnitDto.IsActive,
                Make = truckUnitDto.Make,
                Model = truckUnitDto.Model,
                Vin = truckUnitDto.Vin,
                Plate = truckUnitDto.PlateNumber,
                CurrentHours = truckUnitDto.CumulativeHours ?? 0,
                CurrentMileage = truckUnitDto.Odometer ?? 0,
                FuelCapacity = (int?)truckUnitDto.FuelCapacity ?? 0,
            };
            return truck;
        }

        public static TruckUnitDto ParseToTruckUnitDto(this Truck truck)
        {
            var truckUnitDto = new TruckUnitDto
            {
                Id = truck.Id,
                Vin = truck.Vin,
                Name = truck.TruckCode,
                Make = truck.Make,
                Model = truck.Model,
                PlateNumber = truck.Plate,
                IsActive = truck.IsActive,
                CumulativeHours = truck.CurrentHours,
                Odometer = (decimal?)UnitConverter.ConvertMilesToMeters((double)truck.CurrentMileage),
                FuelCapacity = truck.FuelCapacity,
                GrossWeight = (int)truck.CargoCapacity,
                MakeModelText = $"{truck.Make} {truck.Model}",
            };
            return truckUnitDto;
        }
    }
}
