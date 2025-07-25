using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Charges;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.Locations;
using DispatcherWeb.PayStatements;
using DispatcherWeb.Quotes;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Orders
{
    [Table("OrderLine")]
    public class OrderLine : FullAuditedEntity, IMustHaveTenant
    {
        public OrderLine()
        {
            OrderLineTrucks = new HashSet<OrderLineTruck>();
            Tickets = new HashSet<Ticket>();
            Dispatches = new HashSet<Dispatch>();
            ReceiptLines = new HashSet<ReceiptLine>();
            OrderLineVehicleCategories = new HashSet<OrderLineVehicleCategory>();
        }

        public int TenantId { get; set; }

        public int OrderId { get; set; }

        public bool IsComplete { get; set; }

        public bool IsCancelled { get; set; }

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

        public int? FreightItemId { get; set; }

        public int? MaterialItemId { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        public int? MaterialUomId { get; set; }

        public virtual UnitOfMeasure MaterialUom { get; set; }

        public int? FreightUomId { get; set; }

        public virtual UnitOfMeasure FreightUom { get; set; }

        [Required(ErrorMessage = "Designation is a required field")]
        public DesignationEnum Designation { get; set; }

        public decimal MaterialPrice { get; set; }

        [Column(TypeName = "money")]
        public decimal MaterialActualPrice { get; set; }

        public decimal FreightPrice { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }

        public int? DriverPayTimeClassificationId { get; set; }

        public virtual TimeClassification DriverPayTimeClassification { get; set; }

        public decimal? HourlyDriverPayRate { get; set; }

        public short? TravelTime { get; set; }

        [Column(TypeName = "money")]
        public decimal? FuelSurchargeRate { get; set; }

        public bool IsMaterialPriceOverridden { get; set; }

        public bool IsFreightPriceOverridden { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        //Todo
        //[StringLength(EntityStringFieldLengths.OrderLine.DriverNote)]
        //public string DriverNote { get; set; }

        public int? Loads { get; set; }

        public decimal? EstimatedAmount { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public StaggeredTimeKind StaggeredTimeKind { get; set; }

        public int? StaggeredTimeInterval { get; set; } //in minutes

        public DateTime? FirstStaggeredTimeOnJob { get; set; }

        public double? NumberOfTrucks { get; set; }

        public double? ScheduledTrucks { get; set; }

        public bool IsMultipleLoads { get; set; }

        public bool RequireTicket { get; set; }

        public bool RequiresCustomerNotification { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.CustomerNotificationContactName)]
        public string CustomerNotificationContactName { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.CustomerNotificationPhoneNumber)]
        public string CustomerNotificationPhoneNumber { get; set; }

        /// <summary>
        /// MaterialCompany's order line id. Only set for HaulingCompany order lines when a copy of this order line exists on another MaterialCompany tenant.
        /// </summary>
        public int? MaterialCompanyOrderLineId { get; set; }
        /// <summary>
        /// MaterialCompany's tenant id. Only set for HaulingCompany order lines when a copy of this order line exists on another MaterialCompany tenant.
        /// </summary>
        public int? MaterialCompanyTenantId { get; set; }

        /// <summary>
        /// HaulingCompany's order line id. Only set for MaterialCompany order lines when a copy of this order line exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyOrderLineId { get; set; }
        /// <summary>
        /// HaulingCompany's tenant id. Only set for MaterialCompany order lines when a copy of this order line exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyTenantId { get; set; }

        public int? QuoteLineId { get; set; }

        public BedConstructionEnum? BedConstruction { get; set; }

        public virtual QuoteLine QuoteLine { get; set; }

        public virtual Order Order { get; set; }

        public virtual Location LoadAt { get; set; }

        public virtual Location DeliverTo { get; set; }

        public virtual Item FreightItem { get; set; }

        public virtual Item MaterialItem { get; set; }

        public bool ProductionPay { get; set; }

        public bool LoadBased { get; set; }

        public virtual ICollection<OrderLineTruck> OrderLineTrucks { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }

        public virtual ICollection<Dispatch> Dispatches { get; set; }

        public virtual ICollection<ReceiptLine> ReceiptLines { get; set; }

        public virtual ICollection<Charge> Charges { get; set; }

        public virtual ICollection<EmployeeTime> EmployeeTimes { get; set; }

        public virtual ICollection<OrderLineVehicleCategory> OrderLineVehicleCategories { get; set; }

        public virtual ICollection<LeaseHaulerRequest> LeaseHaulerRequests { get; set; }

        public virtual ICollection<PayStatementTime> PayStatementTimes { get; set; }
    }
}
