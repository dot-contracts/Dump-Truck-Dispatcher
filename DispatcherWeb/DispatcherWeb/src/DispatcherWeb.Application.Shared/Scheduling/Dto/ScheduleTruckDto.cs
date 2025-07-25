using System;
using System.Collections.Generic;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleTruckDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public int? OfficeId { get; set; }
        public VehicleCategoryDto VehicleCategory { get; set; }
        public bool AlwaysShowOnSchedule { get; set; }
        public bool CanPullTrailer { get; set; }
        public bool IsOutOfService { get; set; }
        public bool IsActive { get; set; }
        public decimal Utilization { get; set; }
        public decimal ActualUtilization { get; set; }
        public IList<decimal> UtilizationList { get; set; }
        public bool HasDefaultDriver => DefaultDriverId.HasValue;
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public bool HasNoDriver { get; set; }
        public bool HasDriverAssignment { get; set; }
        public DateTime? DriverDateOfHire { get; set; }
        public bool IsExternal { get; set; }
        public int? LeaseHaulerId { get; set; }
        public BedConstructionEnum? BedConstruction { get; set; }
        public string BedConstructionFormatted => BedConstruction.GetDisplayName();
        public int? Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public bool IsApportioned { get; set; }
        public string DefaultDriverName { get; set; }
        public int? DefaultDriverId { get; set; }
        public DateTime? DefaultDriverDateOfHire { get; set; }
        public List<InsuranceDto> Insurances { get; set; }

        public ScheduleTruckTractorDto Tractor { get; set; }
        public ScheduleTruckTrailerDto Trailer { get; set; }
    }
}
