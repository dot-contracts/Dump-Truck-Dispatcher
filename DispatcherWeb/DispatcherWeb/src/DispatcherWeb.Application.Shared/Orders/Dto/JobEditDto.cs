using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Orders.Dto
{
    public class JobEditDto
    {
        public int? OrderId { get; set; }

        public int? OrderLineId { get; set; }

        [Required]
        public DateTime? DeliveryDate { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public bool CustomerIsCod { get; set; }

        public OrderPriority Priority { get; set; }


        [StringLength(EntityStringFieldLengths.Order.ChargeTo)]
        public string ChargeTo { get; set; }

        [StringLength(EntityStringFieldLengths.Order.PoNumber)]
        public string PONumber { get; set; }

        [StringLength(EntityStringFieldLengths.Order.SpectrumNumber)]
        public string SpectrumNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Order.Directions)]
        public string Directions { get; set; }

        public int? QuoteId { get; set; }

        public string QuoteName { get; set; }

        public decimal? OrderLineSalesTax { get; set; }

        public decimal? OrderLineTotal { get; set; }

        public Shift? Shift { get; set; }

        [Required(ErrorMessage = "Office is a required field")]
        public int OfficeId { get; set; }

        public string OfficeName { get; set; }

        public bool IsSingleOffice { get; set; }

        public int? ContactId { get; set; }

        public string ContactName { get; set; }

        public string ContactPhone { get; set; }

        //public int LineNumber { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialPricePerUnit { get; set; }

        public decimal? MaterialCostRate { get; set; }

        public decimal? FreightPricePerUnit { get; set; }

        public bool IsMaterialPricePerUnitOverridden { get; set; }

        public bool IsFreightPricePerUnitOverridden { get; set; }

        public bool IsFreightRateToPayDriversOverridden { get; set; }

        public bool IsLeaseHaulerPriceOverridden { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public decimal? HourlyDriverPayRate { get; set; }

        public short? TravelTime { get; set; }

        public int? DriverPayTimeClassificationId { get; set; }

        public string DriverPayTimeClassificationName { get; set; }

        public bool? UseZoneBasedRates { get; set; }

        public int? FreightItemId { get; set; }

        public int? MaterialItemId { get; set; }

        public string MaterialItemName { get; set; }

        public string FreightItemName { get; set; }

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

        public decimal SalesTaxRate { get; set; }

        public decimal SalesTax { get; set; }

        public int? SalesTaxEntityId { get; set; }

        public string SalesTaxEntityName { get; set; }

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

        public bool AutoGenerateTicketNumber { get; set; }

        public int? TicketId { get; set; }

        [StringLength(EntityStringFieldLengths.Ticket.TicketNumber)]
        public string TicketNumber { get; set; }

        public bool HasQuoteBasedPricing { get; set; }
        public bool HasTickets { get; set; }
        public bool HasOpenDispatches { get; set; }
        public DateTime? FirstStaggeredTimeOnJob { get; set; }
        public int? StaggeredTimeInterval { get; set; }
        public bool UpdateStaggeredTime { get; set; }
        public int? QuoteLineId { get; set; }
        public string FocusFieldId { get; set; }
        public int? MaterialCompanyOrderId { get; set; }
        public int? DefaultLoadAtLocationId { get; set; }
        public string DefaultLoadAtLocationName { get; set; }
        public int? DefaultMaterialItemId { get; set; }
        public string DefaultMaterialItemName { get; set; }
        public int? DefaultMaterialUomId { get; set; }
        public string DefaultMaterialUomName { get; set; }

        public int? FuelSurchargeCalculationId { get; set; }

        public string FuelSurchargeCalculationName { get; set; }

        public bool? CanChangeBaseFuelCost { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal? BaseFuelCost { get; set; }

        public string DefaultFuelSurchargeCalculationName { get; set; }

        public decimal? DefaultBaseFuelCost { get; set; }

        public bool? DefaultCanChangeBaseFuelCost { get; set; }

        public bool RequiresCustomerNotification { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.CustomerNotificationContactName)]
        public string CustomerNotificationContactName { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.CustomerNotificationPhoneNumber)]
        public string CustomerNotificationPhoneNumber { get; set; }

        public List<OrderLineVehicleCategoryDto> VehicleCategories { get; set; }

        public BedConstructionEnum? BedConstruction { get; set; }

        public int? PricingTierId { get; set; }

        public bool? CustomerIsTaxExempt { get; set; }

        public bool? QuoteIsTaxExempt { get; set; }

        public bool IsTaxExempt { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }
    }
}
