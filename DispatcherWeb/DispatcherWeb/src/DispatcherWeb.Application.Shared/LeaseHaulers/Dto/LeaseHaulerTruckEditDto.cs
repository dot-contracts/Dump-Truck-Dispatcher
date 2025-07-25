using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerTruckEditDto : IOfficeIdNameDto
    {
        public int? Id { get; set; }

        [Required]
        public int LeaseHaulerId { get; set; }

        [Required(ErrorMessage = "Truck Code is a required field")]
        [StringLength(25)]
        public string TruckCode { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Plate)]
        public string LicensePlate { get; set; }

        public int VehicleCategoryId { get; set; }
        public string VehicleCategoryName { get; set; }

        public bool? VehicleCategoryIsPowered { get; set; }

        public AssetType VehicleCategoryAssetType { get; set; }

        public bool CanPullTrailer { get; set; }

        public bool AlwaysShowOnSchedule { get; set; }

        public int? OfficeId { get; set; }

        public string OfficeName { get; set; }

        public int? DefaultDriverId { get; set; }

        public string DefaultDriverName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? InactivationDate { get; set; }

        public bool IsSingleOffice { get; set; }

        public int? HaulingCompanyTruckId { get; set; }

        public int? CurrentTrailerId { get; set; }
        public string CurrentTrailerCode { get; set; }

        public BedConstructionEnum? BedConstruction { get; set; }

        public bool IsApportioned { get; set; }

        int IOfficeIdNameDto.OfficeId { get => OfficeId ?? 0; set => OfficeId = value; }
    }
}
