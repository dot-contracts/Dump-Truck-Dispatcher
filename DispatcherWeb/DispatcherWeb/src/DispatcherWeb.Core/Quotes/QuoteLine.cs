using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Items;
using DispatcherWeb.Locations;
using DispatcherWeb.Orders;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Quotes
{
    [Table("QuoteLine")]
    public class QuoteLine : FullAuditedEntity, IMustHaveTenant
    {
        public QuoteLine()
        {
            QuoteLineVehicleCategories = new HashSet<QuoteLineVehicleCategory>();
        }

        public int TenantId { get; set; }

        public int? FreightItemId { get; set; }

        public int? MaterialItemId { get; set; }

        public int QuoteId { get; set; }

        public int? MaterialUomId { get; set; }

        public int? FreightUomId { get; set; }

        public DesignationEnum Designation { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        [Column(TypeName = "money")]
        public decimal? PricePerUnit { get; set; }

        public bool IsPricePerUnitOverridden { get; set; }

        public decimal? MaterialCostRate { get; set; }

        [Column(TypeName = "money")]
        public decimal? FreightRate { get; set; }

        public bool IsFreightRateOverridden { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public bool IsLeaseHaulerRateOverridden { get; set; }

        public decimal? FreightRateToPayDrivers { get; set; }

        public bool IsFreightRateToPayDriversOverridden { get; set; }

        public int? DriverPayTimeClassificationId { get; set; }

        public virtual TimeClassification DriverPayTimeClassification { get; set; }

        public decimal? HourlyDriverPayRate { get; set; }

        public short? TravelTime { get; set; }

        public bool ProductionPay { get; set; }

        public bool LoadBased { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
        public string JobNumber { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }

        public BedConstructionEnum? BedConstruction { get; set; }

        public virtual ICollection<OrderLine> OrderLines { get; set; }

        public virtual UnitOfMeasure MaterialUom { get; set; }

        public virtual UnitOfMeasure FreightUom { get; set; }

        public virtual Quote Quote { get; set; }

        public virtual Item FreightItem { get; set; }

        public virtual Item MaterialItem { get; set; }

        public virtual Location LoadAt { get; set; }

        public virtual Location DeliverTo { get; set; }

        public bool RequireTicket { get; set; }

        public virtual ICollection<QuoteLineVehicleCategory> QuoteLineVehicleCategories { get; set; }

        public QuoteLine Clone()
        {
            return new QuoteLine
            {
                CreationTime = CreationTime,
                CreatorUserId = CreatorUserId,
                DeleterUserId = DeleterUserId,
                DeletionTime = DeletionTime,
                DeliverToId = DeliverToId,
                Designation = Designation,
                FreightQuantity = FreightQuantity,
                FreightRate = FreightRate,
                FreightRateToPayDrivers = FreightRateToPayDrivers,
                DriverPayTimeClassificationId = DriverPayTimeClassificationId,
                HourlyDriverPayRate = HourlyDriverPayRate,
                TravelTime = TravelTime,
                ProductionPay = ProductionPay,
                RequireTicket = RequireTicket,
                LoadBased = LoadBased,
                FreightUomId = FreightUomId,
                Id = Id,
                IsDeleted = IsDeleted,
                IsPricePerUnitOverridden = IsPricePerUnitOverridden,
                IsFreightRateOverridden = IsFreightRateOverridden,
                IsFreightRateToPayDriversOverridden = IsFreightRateToPayDriversOverridden,
                IsLeaseHaulerRateOverridden = IsLeaseHaulerRateOverridden,
                JobNumber = JobNumber,
                LastModificationTime = LastModificationTime,
                LastModifierUserId = LastModifierUserId,
                LeaseHaulerRate = LeaseHaulerRate,
                LoadAtId = LoadAtId,
                MaterialQuantity = MaterialQuantity,
                MaterialUomId = MaterialUomId,
                Note = Note,
                PricePerUnit = PricePerUnit,
                MaterialCostRate = MaterialCostRate,
                QuoteId = QuoteId,
                FreightItemId = FreightItemId,
                MaterialItemId = MaterialItemId,
                TenantId = TenantId,
                BedConstruction = BedConstruction,
            };
        }
    }
}
