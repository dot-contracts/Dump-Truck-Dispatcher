using System;
using System.Linq;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class InvoiceLineToUploadDto
    {
        public DateTime? DeliveryDateTime { get; set; }
        public string Description { get; set; }
        public string DescriptionAndTicketWithTruck => GetDescriptionAndTicketWithTruck();
        public decimal Subtotal { get; set; }
        public decimal ExtendedAmount { get; set; }
        public decimal FreightExtendedAmount { get; set; }
        public decimal MaterialExtendedAmount { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? MaterialRate { get; set; }
        public decimal Tax { get; set; }
        public bool? IsFreightTaxable { get; set; }
        public bool? IsMaterialTaxable { get; set; }
        public string LeaseHaulerName { get; set; }
        public short LineNumber { get; set; }
        public string TicketNumber { get; set; }
        public string TruckCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public InvoiceLineItemToUploadDto Item { get; set; }
        public InvoiceLineItemToUploadDto FreightItem { get; set; }
        public InvoiceLineItemToUploadDto MaterialItem { get; set; }
        public InvoiceLineItemToUploadDto SalesTaxItem { get; set; }
        public TicketToUploadDto Ticket { get; set; }

        public string JobNumber { get; set; }
        public bool IsSplitMaterialLine { get; set; }
        public bool IsSplitFreightLine { get; set; }
        public ChildInvoiceLineKind? ChildInvoiceLineKind { get; set; }

        public decimal? Rate
        {
            get
            {
                if (Ticket == null || !Ticket.HasOrderLine)
                {
                    return FreightRate + MaterialRate;
                }

                var (ticketMaterialQuantity, ticketFreightQuantity) = Ticket.GetMaterialAndFreightQuantity();

                decimal? materialRate;
                if (!Ticket.GetAmountTypeToUse().useMaterial)
                {
                    materialRate = null;
                }
                else if (Ticket.IsOrderLineMaterialTotalOverridden == true)
                {
                    if (ticketMaterialQuantity == 0)
                    {
                        materialRate = null;
                    }
                    else
                    {
                        materialRate = Math.Round((Ticket.OrderLineMaterialTotal / ticketMaterialQuantity) ?? 0, 2);
                    }
                }
                else
                {
                    materialRate = MaterialRate;
                }

                decimal? freightRate;
                if (!Ticket.GetAmountTypeToUse().useFreight)
                {
                    freightRate = null;
                }
                else if (Ticket.IsOrderLineFreightTotalOverridden == true)
                {
                    if (ticketFreightQuantity == 0)
                    {
                        freightRate = null;
                    }
                    else
                    {
                        freightRate = Math.Round((Ticket.OrderLineFreightTotal / ticketFreightQuantity) ?? 0, 2);
                    }
                }
                else
                {
                    freightRate = FreightRate;
                }

                return (materialRate ?? 0) + (freightRate ?? 0);
            }
        }

        public decimal? GetTaxableTotal(TaxCalculationType taxCalculationType, bool separateItems)
        {
            var taxableTotal = 0M;

            if (separateItems)
            {
                if (IsMaterialTaxable == true)
                {
                    taxableTotal += MaterialExtendedAmount;
                }

                if (IsFreightTaxable == true)
                {
                    taxableTotal += FreightExtendedAmount;
                }

                return taxableTotal;
            }

            switch (taxCalculationType)
            {
                case TaxCalculationType.FreightAndMaterialTotal:
                    if (IsFreightTaxable == true)
                    {
                        taxableTotal = MaterialExtendedAmount + FreightExtendedAmount;
                    }
                    break;

                case TaxCalculationType.MaterialLineItemsTotal:
                    if (MaterialExtendedAmount > 0 && IsFreightTaxable == true)
                    {
                        taxableTotal = MaterialExtendedAmount + FreightExtendedAmount;
                    }
                    break;

                case TaxCalculationType.MaterialTotal:
                    if (IsFreightTaxable == true)
                    {
                        taxableTotal = MaterialExtendedAmount;
                    }
                    break;
            }
            return taxableTotal;
        }

        private string GetDescriptionAndTicketWithTruck()
        {
            string ticket = null;
            string truck = null;
            if (!string.IsNullOrEmpty(TicketNumber))
            {
                ticket = $"Ticket: {TicketNumber}";
            }
            if (!string.IsNullOrEmpty(TruckCode))
            {
                truck = $"Truck: {TruckCode}";
            }
            var ticketAndTruck = "";
            if (ticket != null || truck != null)
            {
                ticketAndTruck = " " + string.Join(", ", new[] { ticket, truck }.Where(x => !string.IsNullOrEmpty(x)));
            }

            var deliveryDate = "";
            if (DeliveryDateTime != null)
            {
                deliveryDate = DeliveryDateTime?.ToString("d") + " ";
            }

            return deliveryDate + Description + ticketAndTruck;
        }

        public InvoiceLineToUploadDto Clone()
        {
            return new InvoiceLineToUploadDto
            {
                DeliveryDateTime = DeliveryDateTime,
                Description = Description,
                Subtotal = Subtotal,
                ExtendedAmount = ExtendedAmount,
                FreightExtendedAmount = FreightExtendedAmount,
                MaterialExtendedAmount = MaterialExtendedAmount,
                FreightRate = FreightRate,
                MaterialRate = MaterialRate,
                Tax = Tax,
                IsFreightTaxable = IsFreightTaxable,
                IsMaterialTaxable = IsMaterialTaxable,
                LeaseHaulerName = LeaseHaulerName,
                LineNumber = LineNumber,
                TicketNumber = TicketNumber,
                TruckCode = TruckCode,
                Quantity = Quantity,
                FreightQuantity = FreightQuantity,
                MaterialQuantity = MaterialQuantity,
                Item = Item?.Clone(),
                FreightItem = FreightItem?.Clone(),
                MaterialItem = MaterialItem?.Clone(),
                SalesTaxItem = SalesTaxItem?.Clone(),
                Ticket = Ticket?.Clone(),
                JobNumber = JobNumber,
            };
        }
    }
}
