using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class TruckEditDto : IOfficeIdNameDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Truck Code is a required field")]
        [StringLength(25)]
        public string TruckCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Office is a required field")]
        public int OfficeId { get; set; }

        public string OfficeName { get; set; }
        public bool IsSingleOffice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category is a required field")]
        public int VehicleCategoryId { get; set; }

        public string VehicleCategoryName { get; set; }

        public int? DefaultDriverId { get; set; }

        public string DefaultDriverName { get; set; }

        public int? CurrentTrailerId { get; set; }
        public string CurrentTrailerCode { get; set; }

        public bool IsActive { get; set; }
        public DateTime? InactivationDate { get; set; }

        public bool IsOutOfService { get; set; }
        public string Reason { get; set; }

        public bool IsApportioned { get; set; }

        public bool AlwaysShowOnSchedule { get; set; }

        public BedConstructionEnum? BedConstruction { get; set; }

        public bool CanPullTrailer { get; set; }

        public bool? VehicleCategoryIsPowered { get; set; }

        public AssetType? VehicleCategoryAssetType { get; set; }

        public decimal CurrentMileage { get; set; }
        public decimal CurrentHours { get; set; }

        public int? Year { get; set; }

        [StringLength(50)]
        public string Make { get; set; }

        [StringLength(50)]
        public string Model { get; set; }

        [StringLength(50)]
        public string Vin { get; set; }

        [StringLength(20)]
        public string Plate { get; set; }

        public DateTime? PlateExpiration { get; set; }

        public decimal? CargoCapacity { get; set; }

        public decimal? CargoCapacityCyds { get; set; }

        public FuelType? FuelType { get; set; }

        public int? FuelCapacity { get; set; }

        public string SteerTires { get; set; }

        public string DriveAxleTires { get; set; }
        public string DropAxleTires { get; set; }
        public string TrailerTires { get; set; }
        public string Transmission { get; set; }
        public string Engine { get; set; }
        public string RearEnd { get; set; }


        [StringLength(50)]
        public string InsurancePolicyNumber { get; set; }

        public DateTime? InsuranceValidUntil { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public decimal? PurchasePrice { get; set; }

        [Required(ErrorMessage = "In Service Date is a required field")]
        public DateTime InServiceDate { get; set; }

        public DateTime? SoldDate { get; set; }

        public decimal? SoldPrice { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.TruxTruckId)]
        public string TruxTruckId { get; set; }

        public List<TruckFileEditDto> Files { get; set; }
        public List<MobileDeviceEditDto> MobileDevices { get; set; }
        public bool IsGpsIntegrationConfigured { get; set; }
        public string DtdTrackerUniqueId { get; set; }
        public long? DtdTrackerDeviceTypeId { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.DtdTrackerDeviceTypeName)]
        public string DtdTrackerDeviceTypeName { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.DtdTrackerServerAddress)]
        public string DtdTrackerServerAddress { get; set; }

        //[StringLength(EntityStringFieldLengths.Truck.DtdTrackerPassword)]
        public string DtdTrackerPassword { get; set; }

        public bool EnableDriverAppGps { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Notes)]
        public string Notes { get; set; }
    }
}
