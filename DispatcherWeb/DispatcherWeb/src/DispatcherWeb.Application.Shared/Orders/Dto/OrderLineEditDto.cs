using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Orders.Dto
{
    public class OrderLineEditDto : IOrderLineTaxDetails, IOrderLineTaxTotalDetails
    {
        public int? Id { get; set; }

        public int OrderId { get; set; }

        public int? QuoteId { get; set; }

        public int LineNumber { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialPricePerUnit { get; set; }

        public decimal? MaterialCostRate { get; set; }

        public decimal? FreightPricePerUnit { get; set; }

        public bool IsMaterialPricePerUnitOverridden { get; set; }

        public bool IsFreightPricePerUnitOverridden { get; set; }

        public bool IsFreightRateToPayDriversOverridden { get; set; }

        public bool IsLeaseHaulerPriceOverridden { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public decimal? HourlyDriverPayRate { get; set; }

        public short? TravelTime { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }

        public int? DriverPayTimeClassificationId { get; set; }

        public string DriverPayTimeClassificationName { get; set; }

        public bool? UseZoneBasedRates { get; set; }

        public int? FreightItemId { get; set; }

        public string FreightItemName { get; set; }

        public int? MaterialItemId { get; set; }

        public string MaterialItemName { get; set; }

        public bool? IsTaxable { get; set; }

        public bool? IsMaterialTaxable { get; set; }

        public bool? IsFreightTaxable { get; set; }

        public int? LoadAtId { get; set; }

        public string LoadAtName { get; set; }

        public int? DeliverToId { get; set; }

        public string DeliverToName { get; set; }

        //[Required(ErrorMessage = "Material UOM is a required field")]
        public int? MaterialUomId { get; set; }

        public string MaterialUomName { get; set; }

        //[Required(ErrorMessage = "Freight UOM is a required field")]
        public int? FreightUomId { get; set; }

        public string FreightUomName { get; set; }

        public UnitOfMeasureBaseEnum? FreightUomBaseId { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public string DesignationName => Designation.GetDisplayName();

        public decimal MaterialPrice { get; set; }

        public decimal FreightPrice { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        public double? NumberOfTrucks { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public bool? UpdateOrderLineTrucksTimeOnJob { get; set; }

        public bool? UpdateDispatchesTimeOnJob { get; set; }

        public StaggeredTimeKind StaggeredTimeKind { get; set; }

        public bool IsMaterialPriceOverridden { get; set; }

        public bool IsFreightPriceOverridden { get; set; }

        public bool CanOverrideTotals { get; set; }

        public bool IsMultipleLoads { get; set; }

        public bool ProductionPay { get; set; }

        public bool RequireTicket { get; set; }

        public bool LoadBased { get; set; }

        public bool HasQuoteBasedPricing { get; set; }

        public int? PricingTierId { get; set; }

        public bool CustomerIsCod { get; set; }

        public bool HasTickets { get; set; }

        public bool HasOpenDispatches { get; set; }

        public DateTime? FirstStaggeredTimeOnJob { get; set; }

        public int? StaggeredTimeInterval { get; set; }

        public bool UpdateStaggeredTime { get; set; }

        public int? QuoteLineId { get; set; }

        public bool RequiresCustomerNotification { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.CustomerNotificationContactName)]
        public string CustomerNotificationContactName { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.CustomerNotificationPhoneNumber)]
        public string CustomerNotificationPhoneNumber { get; set; }

        public List<OrderLineVehicleCategoryDto> VehicleCategories { get; set; }

        public BedConstructionEnum? BedConstruction { get; set; }

        public string BedConstructionName => BedConstruction.GetDisplayName();

        public decimal Tax { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal OrderSalesTaxRate { get; set; }
    }
}
