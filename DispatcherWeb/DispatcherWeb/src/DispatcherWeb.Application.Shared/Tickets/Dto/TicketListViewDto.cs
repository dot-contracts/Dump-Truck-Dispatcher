using System;
using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketListViewDto : EditTicketFromListInput, IOrderLineTaxTotalDetails, ITicketQuantity
    {
        public DateTime? Date { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Office { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string FreightItemName { get; set; }
        public string MaterialItemName { get; set; }
        public string TicketNumber { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public string FreightUomName { get; set; }
        public string MaterialUomName { get; set; }
        public int? CarrierId { get; set; }
        public string Carrier { get; set; }
        public string Truck { get; set; }
        public string TruckOffice { get; set; }
        public string Trailer { get; set; }
        public string DriverName { get; set; }
        public string DriverOffice { get; set; }
        public string EmployeeId { get; set; }
        public string JobNumber { get; set; }
        public string QuoteName { get; set; }
        public bool IsBilled { get; set; }
        public bool IsInternal { get; set; }
        public int? LoadCount { get; set; }
        public Guid? TicketPhotoId { get; set; }
        public int? ReceiptId { get; set; }
        public int? ReceiptLineId { get; set; }
        public int? InvoiceNumber { get; set; }
        public int? InvoiceLineId { get; set; }
        public bool HasPayStatements { get; set; }
        public bool HasLeaseHaulerStatements { get; set; }

        public Shift? ShiftRaw { get; set; }
        public string Shift { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }

        public DesignationEnum? Designation { get; set; }

        DesignationEnum ITicketQuantity.Designation => Designation ?? 0;

        public int? OrderLineMaterialUomId { get; set; }

        public int? OrderLineFreightUomId { get; set; }

        public int? TicketUomId { get; set; }
        public bool IsImported { get; set; }
        public decimal TicketMaterialRate => MaterialQuantity != 0 ? Math.Round(MaterialTotal / MaterialQuantity ?? 0, 2) : 0;
        public decimal TicketFreightRate => FreightQuantity != 0 ? Math.Round(FreightTotal / FreightQuantity ?? 0, 2) : 0;
        public decimal Revenue { get; set; }
        public bool? ProductionPay { get; set; }
        public int? PayStatementId { get; set; }

        public decimal? MaterialRate { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? MaterialAmount => Math.Round(this.GetMaterialQuantity() * MaterialRate ?? 0, 2);
        public decimal? FreightAmount => Math.Round(this.GetFreightQuantity() * FreightRate ?? 0, 2);
        public decimal? MaterialCostRate { get; set; }
        public decimal? MaterialCost => MaterialQuantity * MaterialCostRate;
        public bool? IsFreightPriceOverridden { get; set; }
        public bool? IsMaterialPriceOverridden { get; set; }
        public decimal? OrderLineFreightPrice { get; set; }
        public decimal? OrderLineMaterialPrice { get; set; }
        public decimal? FuelSurcharge { get; set; }
        public decimal? FreightRateToPayDrivers { get; set; }
        public decimal? DriverPay { get; set; }
        public decimal? DriverPayPercent { get; set; }
        public string DriverPayTimeClassificationName { get; set; }
        public decimal? HourlyDriverPayRate { get; set; }
        public decimal? DriverSpecificHourlyRate { get; set; }
        public decimal FreightTotal => IsFreightPriceOverridden == true ? (OrderLineFreightPrice ?? 0) : Math.Round(this.GetFreightQuantity() * FreightRate ?? 0, 2);
        public decimal MaterialTotal => IsMaterialPriceOverridden == true ? (OrderLineMaterialPrice ?? 0) : Math.Round(this.GetMaterialQuantity() * MaterialRate ?? 0, 2);
        public decimal? PriceOverride
        {
            get
            {
                if (IsFreightPriceOverridden != true && IsMaterialPriceOverridden != true)
                {
                    return null;
                }

                decimal result = 0;
                if (IsFreightPriceOverridden == true && this.GetAmountTypeToUse().useFreight)
                {
                    result += OrderLineFreightPrice ?? 0;
                }

                if (IsMaterialPriceOverridden == true && this.GetAmountTypeToUse().useMaterial)
                {
                    result += OrderLineMaterialPrice ?? 0;
                }

                return result;
            }
        }

        public decimal Tax { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; } //=> MaterialTotal + FreightTotal + Tax;
        decimal IOrderLineTaxTotalDetails.TotalAmount { get => Total; set => Total = value; }
        public decimal? SalesTaxRate { get; set; }

        public bool? IsTaxable { get; set; }
        public bool? IsFreightTaxable { get; set; }
        public bool? IsMaterialTaxable { get; set; }
        public string OrderNote { get; set; }
        public int? OrderId { get; set; }
        public string PONumber { get; set; }
        public string DriverNote { get; set; }
        public decimal? LeaseHaulerRate { get; set; }
        public decimal? LeaseHaulerCost => FreightQuantity * LeaseHaulerRate;
        public string SalesTaxEntityName { get; set; }
        bool? IOrderLineTaxDetails.IsTaxable => IsTaxable ?? false;
        bool? IOrderLineTaxDetails.IsMaterialTaxable => IsMaterialTaxable ?? false;
        bool? IOrderLineTaxDetails.IsFreightTaxable => IsFreightTaxable ?? false;
        decimal IOrderLineTaxDetails.MaterialPrice => MaterialTotal;
        decimal IOrderLineTaxDetails.FreightPrice => FreightTotal;
    }
}
