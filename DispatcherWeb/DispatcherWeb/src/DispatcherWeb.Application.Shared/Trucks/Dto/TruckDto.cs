using System;

namespace DispatcherWeb.Trucks.Dto
{
    public class TruckDto
    {
        public int Id { get; set; }

        public string TruckCode { get; set; }

        public string YearMakeModel { get; set; }

        public string OfficeName { get; set; }

        public string VehicleCategoryName { get; set; }

        public string DefaultDriverName { get; set; }

        public bool IsActive { get; set; }

        public bool IsOutOfService { get; set; }

        public decimal CurrentMileage { get; set; }

        public bool? DueDateStatus { get; set; }
        public bool? DueMileageStatus { get; set; }
        public DateTime? SoldDate { get; set; }
    }
}
