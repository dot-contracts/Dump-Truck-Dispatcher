using System;
using Abp.Extensions;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Tickets;
using Newtonsoft.Json;

namespace DispatcherWeb.Invoices.Dto
{
    public class CustomerTicketDto : IOrderLineTaxTotalDetails, ITicketQuantity, IOrderLineItemWithQuantity
    {
        public int Id { get; set; }

        public int? OrderLineId { get; set; }
        public DateTime? OrderDeliveryDate { get; set; }
        public DateTime? TicketDateTime { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? FreightItemId { get; set; }
        public string FreightItemName { get; set; }
        public int? MaterialItemId { get; set; }
        public string MaterialItemName { get; set; }
        public bool? IsFreightTaxable { get; set; }
        public bool? IsMaterialTaxable { get; set; }

        bool? IOrderLineTaxDetails.IsTaxable => IsFreightTaxable;

        public string TicketNumber { get; set; }
        public int? CarrierId { get; set; }
        public string CarrierName { get; set; }
        public string TruckCode { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? MaterialRate { get; set; }
        public DesignationEnum? Designation { get; set; }
        public string FreightUomName { get; set; }
        public string MaterialUomName { get; set; }
        public int? FreightUomId { get; set; }
        public int? MaterialUomId { get; set; }
        public string OrderLineFreightUomName { get; set; }
        public string OrderLineMaterialUomName { get; set; }
        public int? OrderLineFreightUomId { get; set; }
        public int? OrderLineMaterialUomId { get; set; }
        public int? TicketUomId { get; set; }
        public string JobNumber { get; set; }
        public string PoNumber { get; set; }
        public decimal FuelSurcharge { get; set; }
        public decimal Tax { get; set; }
        public decimal? SalesTaxRate { get; set; }
        public int? SalesTaxEntityId { get; set; }
        public string SalesTaxEntityName { get; set; }

        public bool? IsOrderLineFreightTotalOverridden { get; set; }

        public bool? IsOrderLineMaterialTotalOverridden { get; set; }

        public decimal? OrderLineFreightTotal { get; set; }

        public decimal? OrderLineMaterialTotal { get; set; }

        public decimal FreightTotal => IsOrderLineFreightTotalOverridden == true ? (OrderLineFreightTotal ?? 0) : Math.Round(this.GetFreightQuantity() * FreightRate ?? 0, 2);
        public decimal MaterialTotal => IsOrderLineMaterialTotalOverridden == true ? (OrderLineMaterialTotal ?? 0) : Math.Round(this.GetMaterialQuantity() * MaterialRate ?? 0, 2);

        decimal IOrderLineTaxDetails.MaterialPrice => MaterialTotal;

        decimal IOrderLineTaxDetails.FreightPrice => FreightTotal;
        public decimal Subtotal { get; set; }

        public decimal Total { get; set; } //=> MaterialTotal + FreightTotal + Tax;

        public string LeaseHaulerName { get; set; }

        public int? InvoiceLineId { get; set; }

        public InvoicingMethodEnum InvoicingMethod { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public string LoadAtAndDeliverToDescription
        {
            get
            {
                var designationHasMaterial = Designation?.HasMaterial() == true;

                if (HideLoadAtAndDeliverToOnHourlyInvoices
                    && !designationHasMaterial
                    && OrderLineFreightUomName?.ToLower().StartsWith("hour") == true)
                {
                    return "";
                }

                var result = "";
                if (!string.IsNullOrEmpty(LoadAtName))
                {
                    result += $" from {LoadAtName}";
                }
                if (!string.IsNullOrEmpty(DeliverToName))
                {
                    result += $" to {DeliverToName}";
                }

                return result;
            }
        }

        [JsonIgnore]
        public string JobNumberAndPoNumberDescription
        {
            get
            {
                var jobNumber = JobNumber.IsNullOrEmpty() ? "" : "; Job Nbr: " + JobNumber;
                var poNumber = PoNumber.IsNullOrEmpty() ? "" : "; PO Nbr: " + PoNumber;
                return $"{jobNumber}{poNumber}";
            }
        }

        [JsonIgnore]
        public bool HideLoadAtAndDeliverToOnHourlyInvoices { get; set; }

        decimal IOrderLineTaxTotalDetails.TotalAmount { get => Total; set => Total = value; }

        DesignationEnum ITicketQuantity.Designation => Designation ?? 0;
    }
}
