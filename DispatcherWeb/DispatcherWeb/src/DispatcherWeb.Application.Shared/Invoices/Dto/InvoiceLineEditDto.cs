using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Orders.TaxDetails;

namespace DispatcherWeb.Invoices.Dto
{
    public class InvoiceLineEditDto : IOrderLineTaxTotalDetails
    {
        public int? Id { get; set; }

        public short LineNumber { get; set; }

        public int? TicketId { get; set; }

        public int? ChargeId { get; set; }

        public int? OrderLineId { get; set; }

        public string TicketNumber { get; set; }

        public string JobNumber { get; set; }

        public string PoNumber { get; set; }

        public int? CustomerId { get; set; }

        public decimal? SalesTaxRate { get; set; }

        public int? SalesTaxEntityId { get; set; }

        public string SalesTaxEntityName { get; set; }

        public int? CarrierId { get; set; }

        public string CarrierName { get; set; }

        public DateTime? DeliveryDateTime { get; set; }

        [StringLength(25)]
        public string TruckCode { get; set; }

        public string LeaseHaulerName { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? MaterialRate { get; set; }

        public decimal? FreightRate { get; set; }

        public bool IsFreightRateOverridden { get; set; }

        public int? FreightItemId { get; set; }

        public string FreightItemName { get; set; }

        public int? MaterialItemId { get; set; }

        public string MaterialItemName { get; set; }

        public decimal MaterialExtendedAmount { get; set; }

        public decimal FreightExtendedAmount { get; set; }

        decimal IOrderLineTaxDetails.MaterialPrice => MaterialExtendedAmount;

        decimal IOrderLineTaxDetails.FreightPrice => FreightExtendedAmount;

        public bool? IsFreightTaxable { get; set; }

        public bool? IsMaterialTaxable { get; set; }

        bool? IOrderLineTaxDetails.IsTaxable => IsFreightTaxable;

        bool? IOrderLineTaxDetails.IsFreightTaxable => IsFreightTaxable;

        bool? IOrderLineTaxDetails.IsMaterialTaxable => IsMaterialTaxable;

        public bool? UseMaterialQuantity { get; set; }

        public decimal Tax { get; set; }

        public decimal FuelSurcharge { get; set; }

        public decimal Subtotal { get; set; }

        public decimal ExtendedAmount { get; set; }

        public Guid? Guid { get; set; }

        public Guid? ParentInvoiceLineGuid { get; set; }

        public int? ParentInvoiceLineId { get; set; }

        public ChildInvoiceLineKind? ChildInvoiceLineKind { get; set; }

        decimal IOrderLineTaxTotalDetails.TotalAmount { get => ExtendedAmount; set => ExtendedAmount = value; }
    }
}
