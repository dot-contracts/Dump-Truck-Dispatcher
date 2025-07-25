using System;
using System.Collections.Generic;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class ScheduleOrderLineTruckDto
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public int OrderId { get; set; }

        public int OrderLineId { get; set; }

        public int TruckId { get; set; }

        public string TruckCode { get; set; }

        public string TruckCodeCombined => Trailer != null ? TruckCode + " :: " + Trailer.TruckCode : TruckCode;

        public ScheduleTruckTrailerDto Trailer { get; set; }

        public int? DriverId { get; set; }

        public int? OfficeId { get; set; }

        public bool IsExternal { get; set; }

        public decimal Utilization { get; set; }

        public VehicleCategoryDto VehicleCategory { get; set; }

        public bool AlwaysShowOnSchedule { get; set; }

        public bool CanPullTrailer { get; set; }

        public bool IsDone { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public int? LeaseHaulerId { get; set; }

        public List<ScheduleOrderLineTruckDispatchDto> Dispatches { get; set; }
    }
}
