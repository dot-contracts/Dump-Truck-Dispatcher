using System.Collections.Generic;

namespace DispatcherWeb.VehicleServices.Dto
{
    public class VehicleServiceEditDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int? RecommendedTimeInterval { get; set; }
        public int? RecommendedHourInterval { get; set; }
        public int? RecommendedMileageInterval { get; set; }

        public int? WarningDays { get; set; }
        public int? WarningHours { get; set; }
        public int? WarningMiles { get; set; }

        public List<VehicleServiceDocumentEditDto> Documents { get; set; }
    }
}
