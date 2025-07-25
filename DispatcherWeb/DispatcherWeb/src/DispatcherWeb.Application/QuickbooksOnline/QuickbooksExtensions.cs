using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Extensions;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Invoices;
using DispatcherWeb.Invoices.Dto;
using DispatcherWeb.Items;
using DispatcherWeb.QuickbooksOnline.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.QuickbooksOnline
{
    public static class QuickbooksExtensions
    {
        public static async Task<List<InvoiceToUploadDto<Invoices.Invoice>>> ToInvoiceToUploadList(this IQueryable<Invoices.Invoice> query, string timezone, bool separateItems)
        {
            var invoicesToUpload = await query
                .Select(x => new InvoiceToUploadDto<Invoices.Invoice>
                {
                    Invoice = x,
                    InvoiceId = x.Id,
                    BillingAddress = x.BillingAddress,
                    EmailAddress = x.EmailAddress,
                    Terms = x.Terms,
                    Status = x.Status,
                    Customer = new CustomerToUploadDto
                    {
                        Id = x.CustomerId,
                        Name = x.Customer.Name,
                        AccountNumber = x.Customer.AccountNumber,
                        IsInQuickBooks = x.Customer.IsInQuickBooks,
                        InvoiceEmail = x.Customer.InvoiceEmail,
                        InvoicingMethod = x.Customer.InvoicingMethod,
                        PreferredDeliveryMethod = x.Customer.PreferredDeliveryMethod,
                        BillingAddress = new PhysicalAddressDto
                        {
                            Address1 = x.Customer.BillingAddress1,
                            Address2 = x.Customer.BillingAddress2,
                            City = x.Customer.BillingCity,
                            CountryCode = x.Customer.BillingCountryCode,
                            State = x.Customer.BillingState,
                            ZipCode = x.Customer.BillingZipCode,
                        },
                        ShippingAddress = new PhysicalAddressDto
                        {
                            Address1 = x.Customer.Address1,
                            Address2 = x.Customer.Address2,
                            City = x.Customer.City,
                            CountryCode = x.Customer.CountryCode,
                            State = x.Customer.State,
                            ZipCode = x.Customer.ZipCode,
                        },
                    },
                    DueDate = x.DueDate,
                    IssueDate = x.IssueDate,
                    Message = x.Message,
                    PONumber = x.PoNumber,
                    Tax = x.Tax,
                    TaxRate = x.TaxRate,
                    TaxName = x.SalesTaxEntity.Name,
                    TotalAmount = x.TotalAmount,
                    OfficeName = x.Office.Name,
                    InvoiceLines = x.InvoiceLines.Select(l => new InvoiceLineToUploadDto
                    {
                        DeliveryDateTime = l.DeliveryDateTime,
                        Description = l.Description,
                        Subtotal = l.Subtotal,
                        ExtendedAmount = l.ExtendedAmount,
                        FreightExtendedAmount = l.FreightExtendedAmount,
                        MaterialExtendedAmount = l.MaterialExtendedAmount,
                        FreightRate = l.FreightRate,
                        MaterialRate = l.MaterialRate,
                        Tax = l.Tax,
                        IsFreightTaxable = l.IsFreightTaxable,
                        IsMaterialTaxable = l.IsMaterialTaxable,
                        JobNumber = l.JobNumber,
                        LeaseHaulerName = l.Ticket.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                        LineNumber = l.LineNumber,
                        TicketNumber = l.Ticket.TicketNumber,
                        TruckCode = l.TruckCode,
                        Quantity = 0,
                        FreightQuantity = l.FreightQuantity,
                        MaterialQuantity = l.MaterialQuantity,
                        FreightItem = l.FreightItem == null ? null : new InvoiceLineItemToUploadDto
                        {
                            Id = l.FreightItem.Id,
                            Name = l.FreightItem.Name,
                            Description = l.FreightItem.Description,
                            IsInQuickBooks = l.FreightItem.IsInQuickBooks,
                            Type = l.FreightItem.Type,
                            IncomeAccount = l.FreightItem.IncomeAccount,
                        },
                        MaterialItem = l.MaterialItem == null ? null : new InvoiceLineItemToUploadDto
                        {
                            Id = l.MaterialItem.Id,
                            Name = l.MaterialItem.Name,
                            Description = l.MaterialItem.Description,
                            IsInQuickBooks = l.MaterialItem.IsInQuickBooks,
                            Type = l.MaterialItem.Type,
                            IncomeAccount = l.MaterialItem.IncomeAccount,
                        },
                        ChildInvoiceLineKind = l.ChildInvoiceLineKind,
                        Ticket = l.Ticket != null ? new TicketToUploadDto
                        {
                            OrderDeliveryDate = l.Ticket.OrderLine.Order.DeliveryDate,
                            TicketDateTimeUtc = l.Ticket.TicketDateTime,
                            OrderLineMaterialUomId = l.Ticket.OrderLine.MaterialUomId,
                            OrderLineFreightUomId = l.Ticket.OrderLine.FreightUomId,
                            TicketUomId = l.Ticket.FreightUomId,
                            TicketUomName = l.Ticket.FreightUom.Name,
                            TicketFreightUomName = l.Ticket.FreightUom.Name,
                            TicketMaterialUomName = l.Ticket.MaterialUom.Name,
                            IsOrderLineMaterialTotalOverridden = l.Ticket.OrderLine.IsMaterialPriceOverridden,
                            IsOrderLineFreightTotalOverridden = l.Ticket.OrderLine.IsFreightPriceOverridden,
                            OrderLineMaterialTotal = l.Ticket.OrderLine.MaterialPrice,
                            OrderLineFreightTotal = l.Ticket.OrderLine.FreightPrice,
                            OrderId = l.Ticket.OrderLine.OrderId,
                            Designation = l.Ticket.OrderLine.Designation,
                            LoadAt = l.Ticket.OrderLine.LoadAt == null ? null : new LocationNameDto
                            {
                                Name = l.Ticket.OrderLine.LoadAt.Name,
                                StreetAddress = l.Ticket.OrderLine.LoadAt.StreetAddress,
                                City = l.Ticket.OrderLine.LoadAt.City,
                                State = l.Ticket.OrderLine.LoadAt.State,
                                ZipCode = l.Ticket.OrderLine.LoadAt.ZipCode,
                            },
                            DeliverTo = l.Ticket.OrderLine.DeliverTo == null ? null : new LocationNameDto
                            {
                                Name = l.Ticket.OrderLine.DeliverTo.Name,
                                StreetAddress = l.Ticket.OrderLine.DeliverTo.StreetAddress,
                                City = l.Ticket.OrderLine.DeliverTo.City,
                                State = l.Ticket.OrderLine.DeliverTo.State,
                                ZipCode = l.Ticket.OrderLine.DeliverTo.ZipCode,
                            },
                            FreightQuantity = l.Ticket.FreightQuantity,
                            MaterialQuantity = l.Ticket.MaterialQuantity,
                            HasOrderLine = l.Ticket.OrderLine != null,
                            TruckCode = l.Ticket.TruckCode,
                            CarrierId = l.Ticket.CarrierId,
                            CarrierName = l.Ticket.Carrier.Name,
                            LeaseHaulerRate = l.Ticket.OrderLine.LeaseHaulerRate,
                            //OrderMaterialPrice = l.Ticket.OrderLine.MaterialPricePerUnit,
                            //OrderFreightPrice = l.Ticket.OrderLine.FreightPricePerUnit
                        } : null,
                    }).ToList(),
                }).ToListAsync();

            foreach (var invoice in invoicesToUpload)
            {
                foreach (var line in invoice.InvoiceLines)
                {
                    line.DeliveryDateTime = line.DeliveryDateTime?.ConvertTimeZoneTo(timezone);

                    line.Item = line.FreightItem;
                    if (!separateItems)
                    {
                        line.FreightItem = null;
                        line.MaterialItem = null;

                        switch (line.Item?.Type)
                        {
                            case ItemType.InventoryPart:
                            case ItemType.NonInventoryPart:
                                line.MaterialItem = line.Item;
                                break;

                            case ItemType.Service:
                                line.FreightItem = line.Item;
                                break;
                        }
                    }
                }

                invoice.InvoiceLines = invoice.InvoiceLines.OrderBy(x => x.LineNumber).ToList();
            }

            invoicesToUpload.ForEach(i =>
                i.InvoiceLines.RemoveAll(l =>
                    l.ChildInvoiceLineKind.IsIn(ChildInvoiceLineKind.BottomFuelSurchargeLine, ChildInvoiceLineKind.FuelSurchargeLinePerTicket)
                    && l.ExtendedAmount == 0));

            return invoicesToUpload;
        }

        public static void SplitMaterialAndFreightLines(this List<InvoiceToUploadDto<Invoice>> invoicesToUpload, bool separateItems, TaxCalculationType taxCalculationType, bool alwaysShowFreightAndMaterialOnSeparateLines)
        {
            foreach (var invoice in invoicesToUpload)
            {
                var newLinesList = new List<InvoiceLineToUploadDto>();
                foreach (var invoiceLine in invoice.InvoiceLines)
                {
                    if (!separateItems
                        && invoiceLine.MaterialQuantity == invoiceLine.FreightQuantity
                        && (invoiceLine.MaterialExtendedAmount == 0 || invoiceLine.FreightExtendedAmount == 0 || invoiceLine.IsFreightTaxable == false))
                    {
                        //no need to split lines with only either material or freight total
                        invoiceLine.Quantity = invoiceLine.FreightQuantity ?? 0;
                        newLinesList.Add(invoiceLine);
                        continue;
                    }

                    if (!separateItems)
                    {
                        invoiceLine.IsMaterialTaxable = invoiceLine.IsFreightTaxable;
                        if (taxCalculationType == TaxCalculationType.MaterialTotal)
                        {
                            invoiceLine.IsFreightTaxable = false;
                        }
                    }

                    if (!alwaysShowFreightAndMaterialOnSeparateLines
                        && invoiceLine.FreightQuantity == invoiceLine.MaterialQuantity
                        && invoiceLine.Ticket?.TicketFreightUomName == invoiceLine.Ticket?.TicketMaterialUomName
                        && invoiceLine.IsFreightTaxable == invoiceLine.IsMaterialTaxable
                    )
                    {
                        invoiceLine.Quantity = invoiceLine.FreightQuantity ?? 0;
                        //if (!string.IsNullOrEmpty(invoiceLine.MaterialItem?.Name) && invoiceLine.Item != null)
                        //{
                        //    invoiceLine.Item.Name += " of " + invoiceLine.MaterialItem.Name;
                        //}
                        //we're exporting these materials/services as well, so can't modify their names without some consequences

                        newLinesList.Add(invoiceLine);
                        continue;
                    }

                    if (invoiceLine.MaterialExtendedAmount == 0)
                    {
                        invoiceLine.Quantity = invoiceLine.FreightQuantity ?? 0;
                        newLinesList.Add(invoiceLine);
                        continue;
                    }

                    if (invoiceLine.FreightExtendedAmount == 0)
                    {
                        if (separateItems)
                        {
                            invoiceLine.Item = invoiceLine.MaterialItem ?? invoiceLine.FreightItem;
                        }
                        invoiceLine.Quantity = invoiceLine.MaterialQuantity ?? 0;
                        newLinesList.Add(invoiceLine);
                        continue;
                    }

                    var materialLine = invoiceLine.Clone();
                    materialLine.FreightRate = 0;
                    if (materialLine.Ticket != null)
                    {
                        materialLine.Ticket.OrderLineFreightUomId = null;
                        materialLine.Ticket.OrderLineFreightTotal = null;
                        materialLine.Ticket.IsOrderLineFreightTotalOverridden = false;
                        materialLine.Ticket.TicketUomName = materialLine.Ticket.TicketMaterialUomName;
                    }
                    materialLine.Subtotal -= materialLine.FreightExtendedAmount;
                    materialLine.ExtendedAmount -= materialLine.FreightExtendedAmount;
                    materialLine.FreightExtendedAmount = 0;
                    materialLine.IsSplitMaterialLine = true;

                    materialLine.Quantity = materialLine.MaterialQuantity ?? 0;

                    var newMaterialTax = materialLine.IsMaterialTaxable == true ? materialLine.MaterialExtendedAmount * invoice.TaxRate / 100 : 0;
                    materialLine.ExtendedAmount -= materialLine.Tax;
                    materialLine.ExtendedAmount += newMaterialTax;
                    materialLine.Tax = newMaterialTax;

                    materialLine.FreightQuantity = 0;

                    if (separateItems)
                    {
                        materialLine.IsFreightTaxable = false;

                        materialLine.Item = materialLine.MaterialItem ?? materialLine.FreightItem;
                    }
                    else
                    {
                        if (materialLine.Item != null)
                        {
                            materialLine.Item.Description = "Material " + materialLine.Item.Description;
                        }
                    }
                    if ((materialLine.MaterialQuantity ?? 0) != 0)
                    {
                        newLinesList.Add(materialLine);
                    }

                    var freightLine = invoiceLine.Clone();
                    freightLine.MaterialRate = 0;
                    if (freightLine.Ticket != null)
                    {
                        freightLine.Ticket.OrderLineMaterialUomId = null;
                        freightLine.Ticket.OrderLineMaterialTotal = null;
                        freightLine.Ticket.IsOrderLineMaterialTotalOverridden = false;
                        freightLine.Ticket.TicketUomName = freightLine.Ticket.TicketFreightUomName;
                    }
                    freightLine.Subtotal -= freightLine.MaterialExtendedAmount;
                    freightLine.ExtendedAmount -= freightLine.MaterialExtendedAmount;
                    freightLine.MaterialExtendedAmount = 0;
                    //freightLine.IsTaxable = false;
                    freightLine.IsSplitFreightLine = true;

                    freightLine.Quantity = freightLine.FreightQuantity ?? 0;

                    var newFreightTax = freightLine.IsFreightTaxable == true ? freightLine.FreightExtendedAmount * invoice.TaxRate / 100 : 0;
                    freightLine.ExtendedAmount -= freightLine.Tax;
                    freightLine.ExtendedAmount += newFreightTax;
                    freightLine.Tax = newFreightTax;
                    freightLine.IsMaterialTaxable = false;

                    freightLine.MaterialQuantity = 0;

                    if (separateItems)
                    {
                        //freightLine.MaterialItem = null;
                        freightLine.Item = freightLine.FreightItem;
                    }
                    else
                    {
                        if (freightLine.Item != null)
                        {
                            freightLine.Item.Description = "Freight " + freightLine.Item.Description;
                        }
                    }
                    if ((freightLine.FreightQuantity ?? 0) != 0)
                    {
                        newLinesList.Add(freightLine);
                    }
                }

                short lineNumber = 1;
                foreach (var newLine in newLinesList)
                {
                    newLine.LineNumber = lineNumber++;
                }

                invoice.InvoiceLines = newLinesList;
            }
        }

        public static void SplitMaterialAndFreightLines(this List<InvoicePrintOutDto> invoicesToUpload, bool separateItems, bool alwaysShowFreightAndMaterialOnSeparateLines)
        {
            foreach (var invoice in invoicesToUpload)
            {
                var newLinesList = new List<InvoicePrintOutLineItemDto>();
                foreach (var invoiceLine in invoice.InvoiceLines)
                {
                    if (!alwaysShowFreightAndMaterialOnSeparateLines
                        && invoiceLine.FreightQuantity == invoiceLine.MaterialQuantity
                    )
                    {
                        if (separateItems && !string.IsNullOrEmpty(invoiceLine.MaterialItemName))
                        {
                            invoiceLine.ItemName += " of " + invoiceLine.MaterialItemName;
                        }
                        invoiceLine.Quantity = invoiceLine.FreightQuantity ?? 0;
                        newLinesList.Add(invoiceLine);
                        continue;
                    }

                    if (invoiceLine.MaterialExtendedAmount == 0)
                    {
                        if (separateItems && !string.IsNullOrEmpty(invoiceLine.MaterialItemName))
                        {
                            invoiceLine.ItemName += " of " + invoiceLine.MaterialItemName;
                        }
                        invoiceLine.Quantity = invoiceLine.FreightQuantity ?? 0;
                        newLinesList.Add(invoiceLine);
                        continue;
                    }

                    if (invoiceLine.FreightExtendedAmount == 0)
                    {
                        if (separateItems)
                        {
                            invoiceLine.ItemId = invoiceLine.MaterialItemId;
                            invoiceLine.ItemName = invoiceLine.MaterialItemName;
                        }

                        invoiceLine.Quantity = invoiceLine.MaterialQuantity ?? 0;
                        newLinesList.Add(invoiceLine);
                        continue;
                    }

                    var materialLine = invoiceLine.Clone();
                    if (separateItems)
                    {
                        materialLine.ItemId = materialLine.MaterialItemId;
                        materialLine.ItemName = materialLine.MaterialItemName;
                    }
                    materialLine.Quantity = materialLine.MaterialQuantity ?? 0;
                    materialLine.FreightRate = 0;
                    materialLine.Subtotal -= materialLine.FreightExtendedAmount;
                    materialLine.ExtendedAmount -= materialLine.FreightExtendedAmount;
                    materialLine.FreightExtendedAmount = 0;
                    materialLine.FuelSurcharge = 0;

                    var newMaterialTax = materialLine.Tax > 0 ? materialLine.MaterialExtendedAmount * invoice.TaxRate / 100 : 0;
                    materialLine.ExtendedAmount -= materialLine.Tax;
                    materialLine.ExtendedAmount += newMaterialTax;
                    materialLine.Tax = newMaterialTax;

                    materialLine.FreightQuantity = 0;

                    if ((materialLine.MaterialQuantity ?? 0) != 0)
                    {
                        newLinesList.Add(materialLine);
                    }

                    var freightLine = invoiceLine.Clone();
                    if (!string.IsNullOrEmpty(freightLine.MaterialItemName))
                    {
                        freightLine.ItemName += " of " + freightLine.MaterialItemName;
                    }
                    freightLine.Quantity = freightLine.FreightQuantity ?? 0;
                    freightLine.MaterialRate = 0;
                    freightLine.Subtotal -= freightLine.MaterialExtendedAmount;
                    freightLine.ExtendedAmount -= freightLine.MaterialExtendedAmount;
                    freightLine.MaterialExtendedAmount = 0;

                    var newFreightTax = freightLine.Tax > 0 ? freightLine.FreightExtendedAmount * invoice.TaxRate / 100 : 0;
                    freightLine.ExtendedAmount -= freightLine.Tax;
                    freightLine.ExtendedAmount += newFreightTax;
                    freightLine.Tax = newFreightTax;

                    freightLine.MaterialQuantity = 0;

                    if ((freightLine.FreightQuantity ?? 0) != 0)
                    {
                        newLinesList.Add(freightLine);
                    }
                }

                short lineNumber = 1;
                foreach (var newLine in newLinesList)
                {
                    newLine.LineNumber = lineNumber++;
                }

                invoice.InvoiceLines = newLinesList;
            }
        }

        public static async Task<InvoiceLineItemToUploadDto> GetSalesTaxItem(this IQueryable<Item> items)
        {
            return await items
                .Where(x => x.Type == ItemType.SalesTaxItem)
                .Select(x => new InvoiceLineItemToUploadDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsInQuickBooks = x.IsInQuickBooks,
                    Type = x.Type,
                    IncomeAccount = x.IncomeAccount,
                })
                .FirstOrDefaultAsync();
        }
    }
}
