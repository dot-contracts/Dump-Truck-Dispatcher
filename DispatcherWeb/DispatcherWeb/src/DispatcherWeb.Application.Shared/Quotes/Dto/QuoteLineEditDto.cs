using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteLineEditDto
    {
        public int? Id { get; set; }

        public int QuoteId { get; set; }

        public bool? UseZoneBasedRates { get; set; }

        public int? FreightItemId { get; set; }

        public string FreightItemName { get; set; }

        public int? MaterialItemId { get; set; }

        public string MaterialItemName { get; set; }

        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }

        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        public UnitOfMeasureBaseEnum? FreightUomBaseId { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtName { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToName { get; set; }

        public decimal? PricePerUnit { get; set; }

        public bool IsPricePerUnitOverridden { get; set; }

        public decimal? MaterialCostRate { get; set; }

        public decimal? FreightRate { get; set; }

        public bool IsFreightRateOverridden { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public decimal? HourlyDriverPayRate { get; set; }

        public short? TravelTime { get; set; }

        public bool IsLeaseHaulerRateOverridden { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }

        public bool IsFreightRateToPayDriversOverridden { get; set; }

        public int? DriverPayTimeClassificationId { get; set; }

        public string DriverPayTimeClassificationName { get; set; }

        public bool ProductionPay { get; set; }

        public bool RequireTicket { get; set; }

        public bool LoadBased { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        public List<QuoteLineVehicleCategoryDto> VehicleCategories { get; set; }

        public BedConstructionEnum? BedConstruction { get; set; }

        public int? CustomerPricingTierId { get; set; }

        public bool CustomerIsCod { get; set; }
    }
}
