using System;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class EmployeeTimeSlimEditDto
    {
        public int? Id { get; set; }

        public Guid? Guid { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public int TimeClassificationId { get; set; }

        public int? EquipmentId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
