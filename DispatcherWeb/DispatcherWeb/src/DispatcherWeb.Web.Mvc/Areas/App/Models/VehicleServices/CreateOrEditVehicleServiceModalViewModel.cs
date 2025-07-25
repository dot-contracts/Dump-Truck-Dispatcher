using System.Collections.Generic;
using DispatcherWeb.VehicleServices.Dto;

namespace DispatcherWeb.Web.Areas.App.Models.VehicleServices
{
    public class CreateOrEditVehicleServiceModalViewModel : VehicleServiceEditDto
    {
        public static CreateOrEditVehicleServiceModalViewModel CreateFromDto(VehicleServiceEditDto dto)
        {
            return new CreateOrEditVehicleServiceModalViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                RecommendedMileageInterval = dto.RecommendedMileageInterval,
                RecommendedTimeInterval = dto.RecommendedTimeInterval,
                RecommendedHourInterval = dto.RecommendedHourInterval,
                WarningDays = dto.WarningDays,
                WarningHours = dto.WarningHours,
                WarningMiles = dto.WarningMiles,

                Documents = dto.Documents ?? new List<VehicleServiceDocumentEditDto>(),
            };
        }

    }
}
