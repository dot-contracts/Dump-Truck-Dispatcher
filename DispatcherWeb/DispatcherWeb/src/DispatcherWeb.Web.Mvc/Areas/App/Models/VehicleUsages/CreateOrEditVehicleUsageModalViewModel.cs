using System;
using DispatcherWeb.VehicleUsages.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.VehicleUsages
{
    public class CreateOrEditVehicleUsageModalViewModel
    {
        public int Id { get; set; }
        public int TruckId { get; set; }
        public DateTime ReadingDateTime { get; set; }
        public ReadingType ReadingType { get; set; }
        public decimal? Mileage { get; set; }
        public decimal? EngineHours { get; set; }

        public int? OfficeId { get; set; }
        public string TruckCode { get; set; }

        public static CreateOrEditVehicleUsageModalViewModel CreateFromVehicleUsageEditDto(VehicleUsageEditDto dto)
        {
            return new CreateOrEditVehicleUsageModalViewModel
            {
                Id = dto.Id,
                TruckId = dto.TruckId,
                TruckCode = dto.TruckCode,
                ReadingDateTime = dto.ReadingDateTime,
                ReadingType = dto.ReadingType,
                Mileage = dto.ReadingType == ReadingType.Miles ? dto.Reading : 0,
                EngineHours = dto.ReadingType == ReadingType.Hours ? dto.Reading : 0,
            };
        }
    }
}
