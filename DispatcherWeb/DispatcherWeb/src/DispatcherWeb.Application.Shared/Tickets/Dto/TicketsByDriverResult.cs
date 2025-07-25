using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketsByDriverResult
    {
        public List<OrderLineDto> OrderLines { get; set; }

        public List<DriverDto> Drivers { get; set; }

        public List<TicketDto> Tickets { get; set; }

        public List<DriverAssignmentDto> DriverAssignments { get; set; }

        public bool HasOpenOrders { get; set; }

        public List<TruckDto> Trucks { get; set; }

        public List<LeaseHaulerDto> LeaseHaulers { get; set; }

        public DailyFuelCostDto DailyFuelCost { get; set; }

        public class OrderLineDto
        {
            public int Id { get; set; }

            public bool IsComplete { get; set; }

            public bool IsCancelled { get; set; }

            public Shift? Shift { get; set; }

            public string PoNumber { get; set; }

            public int? SalesTaxEntityId { get; set; }

            public string SalesTaxEntityName { get; set; }

            public List<OrderLineTruckDto> OrderLineTrucks { get; set; }

            public List<ChargeDto> Charges { get; set; }

            public int OrderId { get; set; }


            [StringLength(EntityStringFieldLengths.OrderLine.JobNumber)]
            public string JobNumber { get; set; }

            public int CustomerId { get; set; }

            public int? QuoteId { get; set; }

            public string CustomerName { get; set; }

            public int OfficeId { get; set; }

            public string OfficeName { get; set; }

            public DateTime? OrderDate { get; set; }

            public int? LoadAtId { get; set; }

            public int? DeliverToId { get; set; }

            public string LoadAtName { get; set; }

            public string DeliverToName { get; set; }

            public int? FreightItemId { get; set; }

            public string FreightItemName { get; set; }

            public int? MaterialItemId { get; set; }

            public string MaterialItemName { get; set; }

            public DesignationEnum Designation { get; set; }


            public int? MaterialUomId { get; set; }


            public string MaterialUomName { get; set; }


            public int? FreightUomId { get; set; }


            public string FreightUomName { get; set; }

            public decimal? FreightRate { get; set; }

            public decimal? FreightRateToPayDrivers { get; set; }

            public bool ProductionPay { get; set; }

            public decimal? MaterialRate { get; set; }

            public decimal? LeaseHaulerRate { get; set; }

            public decimal? FuelSurchargeRate { get; set; }

            public decimal MaterialTotal { get; set; }

            public decimal FreightTotal { get; set; }

            public bool IsMaterialTotalOverridden { get; set; }

            public bool IsFreightTotalOverridden { get; set; }

            public string Note { get; set; }

            public bool HasMultipleOrderLines { get; set; }
        }

        public class ChargeDto
        {
            public bool UseMaterialQuantity { get; set; }

            public decimal ChargeAmount { get; set; }

            public decimal Rate { get; set; }
        }

        public class DriverDto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public bool IsActive { get; set; }

            public bool IsExternal { get; set; }

            public int? LeaseHaulerId { get; set; }
        }

        public class TruckDto
        {
            public int Id { get; set; }

            public string TruckCode { get; set; }

            public bool IsActive { get; set; }

            public int? LeaseHaulerId { get; set; }

            public int? DefaultDriverId { get; set; }

            public int? CurrentTrailerId { get; set; }

            public bool CanPullTrailer { get; set; }

            public VehicleCategoryDto VehicleCategory { get; set; }
        }

        public class VehicleCategoryDto
        {
            public AssetType AssetType { get; set; }
        }

        public class TicketDto : ITicketEditQuantity
        {
            public int Id { get; set; }

            public int? OrderLineId { get; set; }

            public string TicketNumber { get; set; }

            public DateTime? TicketDateTime { get; set; }

            public decimal? FreightQuantity { get; set; }

            public decimal? MaterialQuantity { get; set; }

            public int? FreightUomId { get; set; }

            public string FreightUomName { get; set; }

            public int? MaterialUomId { get; set; }

            public string MaterialUomName { get; set; }

            public int? FreightItemId { get; set; }

            public string FreightItemName { get; set; }

            public int? MaterialItemId { get; set; }

            public string MaterialItemName { get; set; }

            public int? TruckId { get; set; }

            public string TruckCode { get; set; } //only as a fallback value when TruckId is null or doesn't belong to a real truck

            public int? TrailerId { get; set; }

            public string TrailerTruckCode { get; set; }

            public int? DriverId { get; set; }

            public Guid? TicketPhotoId { get; set; }

            public int? ReceiptLineId { get; set; }

            public bool IsReadOnly =>
                IsInvoiced
                || HasPayStatementTickets
                || HasReceipt
                || HasLeaseHaulerStatement;

            public bool NonbillableFreight { get; set; }

            public bool NonbillableMaterial { get; set; }

            public bool IsVerified { get; set; }

            public bool IsInternal { get; set; }

            public int? CarrierId { get; set; }

            public int? OfficeId { get; set; }

            public string OfficeName { get; set; }

            public int? LoadCount { get; set; }

            public bool IsInvoiced { get; set; }

            public bool HasPayStatementTickets { get; set; }

            public bool HasReceipt { get; set; }

            public bool HasLeaseHaulerStatement { get; set; }
        }

        public class OrderLineTruckDto
        {
            public int? DriverId { get; set; }

            public int TruckId { get; set; }

            public int? TrailerId { get; set; }

            public int Id { get; set; }

            public string DriverNote { get; set; }
        }

        public class DriverAssignmentDto
        {
            public int Id { get; set; }

            public Shift? Shift { get; set; }

            public int TruckId { get; set; }

            public int? DriverId { get; set; }
        }

        public class LeaseHaulerDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class DailyFuelCostDto
        {
            public DateTime Date { get; set; }

            public decimal Cost { get; set; }
        }
    }
}
