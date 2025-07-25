using System;

namespace DispatcherWeb.Invoices.Dto
{
    public class InvoicePrintOutLineItemDto
    {
        public int Id { get; set; }
        public short? LineNumber { get; set; }
        public int? TicketId { get; set; }
        public int? ChargeId { get; set; }
        public string TicketNumber { get; set; }
        public string LeaseHaulerName { get; set; }
        public string TruckCode { get; set; }
        public DateTime? DeliveryDateTime { get; set; }

        public string Description { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ExtendedAmount { get; set; }
        public decimal MaterialExtendedAmount { get; set; }
        public decimal FreightExtendedAmount { get; set; }
        public decimal Tax { get; set; }
        public int? ItemId { get; set; }
        public string ItemName { get; set; }
        public int? MaterialItemId { get; set; }
        public string MaterialItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? MaterialRate { get; set; }
        public decimal? RateSum => (FreightRate ?? 0) + (MaterialRate ?? 0);
        public ChildInvoiceLineKind? ChildInvoiceLineKind { get; set; }
        public int? ParentInvoiceLineId { get; set; }
        public string JobNumber { get; set; }
        public string PoNumber { get; set; }
        public string DeliverToName { get; set; }
        public string DeliverToDisplayName { get; set; }
        public string DeliverToStreetAddress { get; set; }
        public string DeliverToFormatted => !string.IsNullOrWhiteSpace(DeliverToName) ? DeliverToName : DeliverToStreetAddress;
        public string LoadAtName { get; set; }
        public string LoadAtDisplayName { get; set; }
        public string LoadAtFormatted => !string.IsNullOrWhiteSpace(LoadAtName) ? LoadAtName : LoadAtDisplayName;
        public decimal FuelSurcharge { get; set; }

        public InvoicePrintOutLineItemDto Clone()
        {
            return new InvoicePrintOutLineItemDto
            {
                Id = Id,
                LineNumber = LineNumber,
                TicketId = TicketId,
                ChargeId = ChargeId,
                TicketNumber = TicketNumber,
                LeaseHaulerName = LeaseHaulerName,
                TruckCode = TruckCode,
                DeliveryDateTime = DeliveryDateTime,
                Description = Description,
                Subtotal = Subtotal,
                ExtendedAmount = ExtendedAmount,
                MaterialExtendedAmount = MaterialExtendedAmount,
                FreightExtendedAmount = FreightExtendedAmount,
                Tax = Tax,
                ItemId = ItemId,
                ItemName = ItemName,
                MaterialItemId = MaterialItemId,
                MaterialItemName = MaterialItemName,
                Quantity = Quantity,
                FreightQuantity = FreightQuantity,
                MaterialQuantity = MaterialQuantity,
                FreightRate = FreightRate,
                MaterialRate = MaterialRate,
                ChildInvoiceLineKind = ChildInvoiceLineKind,
                ParentInvoiceLineId = ParentInvoiceLineId,
                JobNumber = JobNumber,
                PoNumber = PoNumber,
                DeliverToStreetAddress = DeliverToStreetAddress,
                DeliverToName = DeliverToName,
                DeliverToDisplayName = DeliverToDisplayName,
                LoadAtName = LoadAtName,
                LoadAtDisplayName = LoadAtDisplayName,
                FuelSurcharge = FuelSurcharge,
            };
        }
    }
}
