using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.QuickbooksOnline.Dto;

namespace DispatcherWeb.QuickbooksOnlineExport
{
    public class TransactionProCsvExporter : CsvExporterBase, ITransactionProCsvExporter
    {
        private readonly ISettingManager _settingManager;
        private readonly IFeatureChecker _featureChecker;

        public TransactionProCsvExporter(
            ISettingManager settingManager,
            IFeatureChecker featureChecker,
            ITempFileCacheManager tempFileCacheManager
            ) : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
            _featureChecker = featureChecker;
        }

        public async Task<FileDto> ExportToFileAsync<T>(List<InvoiceToUploadDto<T>> invoiceList, string filename)
        {
            var invoiceNumberPrefix = await _settingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);
            var timezone = await SettingManager.GetSettingValueAsync(TimingSettingNames.TimeZone);
            var taxCalculationType = (TaxCalculationType)await _settingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType);
            var separateItems = await _featureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var alwaysShowFreightAndMaterialOnSeparateLines = await _settingManager.GetSettingValueAsync<bool>(AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles);

            return await CreateCsvFileAsync(
                filename + ".csv",
                () =>
                {
                    var flatData = invoiceList.SelectMany(x => x.InvoiceLines, (invoice, invoiceLine) => new
                    {
                        //IsFirstRow = invoice.InvoiceLines.IndexOf(invoiceLine) == 0,
                        Invoice = invoice,
                        InvoiceLine = invoiceLine,
                    }).ToList();

                    AddHeaderAndData(flatData,
                        ("RefNumber", x => invoiceNumberPrefix + x.Invoice.InvoiceId),
                        ("OrderId", x => x.InvoiceLine.Ticket?.OrderId.ToString()),
                        ("Customer", x => x.Invoice.Customer.Name),
                        ("CustomerId", x => x.Invoice.Customer.Id.ToString()),
                        ("AccountNbr", x => x.Invoice.Customer.AccountNumber),
                        ("TxnDate", x => x.Invoice.IssueDate?.ToString("d")),
                        ("DueDate", x => x.Invoice.DueDate?.ToString("d")),
                        ("PONumber", x => x.Invoice.PONumber),
                        ("ShipDate", x => x.InvoiceLine.Ticket?.OrderDeliveryDate?.ToString("d")),
                        ("ShipMethodName", x => ""),
                        ("TrackingNum", x => ""),
                        ("SalesTerm", x => x.Invoice.Terms?.GetDisplayName()),
                        ("Location", x => ""),
                        ("Class", x => ""),
                        ("BillAddrLine1", x => x.Invoice.Customer.BillingAddress.Address1),
                        ("BillAddrLine2", x => x.Invoice.Customer.BillingAddress.Address2),
                        ("BillAddrLine3", x => ""),
                        ("BillAddrLine4", x => ""),
                        ("BillAddrCity", x => x.Invoice.Customer.BillingAddress.City),
                        ("BillAddrState", x => x.Invoice.Customer.BillingAddress.State),
                        ("BillAddrPostalCode", x => x.Invoice.Customer.BillingAddress.ZipCode),
                        ("BillAddrCountry", x => x.Invoice.Customer.BillingAddress.CountryCode),
                        ("ShipAddrLine1", x => x.Invoice.Customer.ShippingAddress.Address1),
                        ("ShipAddrLine2", x => x.Invoice.Customer.ShippingAddress.Address2),
                        ("ShipAddrLine3", x => ""),
                        ("ShipAddrLine4", x => ""),
                        ("ShipAddrCity", x => x.Invoice.Customer.ShippingAddress.City),
                        ("ShipAddrState", x => x.Invoice.Customer.ShippingAddress.State),
                        ("ShipAddrPostalCode", x => x.Invoice.Customer.ShippingAddress.ZipCode),
                        ("ShipAddrCountry", x => x.Invoice.Customer.ShippingAddress.CountryCode),
                        ("PrivateNote", x => ""),
                        ("Msg", x => x.Invoice.Message),
                        ("BillEmail", x => x.Invoice.EmailAddress),
                        ("BillEmailCc", x => ""),
                        ("BillEmailBcc", x => ""),
                        ("Currency", x => ""),
                        ("ExchangeRate", x => ""),
                        ("Deposit", x => ""),
                        ("ToBePrinted", x => x.Invoice.Customer.PreferredDeliveryMethod == PreferredBillingDeliveryMethodEnum.Print
                            && x.Invoice.Status != InvoiceStatus.Printed ? "Y" : "N"),
                        ("ToBeEmailed", x => x.Invoice.Customer.PreferredDeliveryMethod == PreferredBillingDeliveryMethodEnum.Email
                            && x.Invoice.Status != InvoiceStatus.Sent ? "Y" : "N"),
                        ("AllowIPNPayment", x => "N"),
                        ("AllowOnlineCreditCardPayment", x => "N"),
                        ("AllowOnlineACHPayment", x => "N"),
                        ("ShipAmt", x => ""),
                        ("ShipItem", x => ""),
                        ("DiscountAmt", x => ""),
                        ("DiscountRate", x => ""),
                        ("TaxName", x => x.Invoice.TaxName),
                        ("TaxRate", x => x.Invoice.TaxRate.ToString()),
                        ("Taxable Sales", x => x.InvoiceLine.GetTaxableTotal(taxCalculationType, separateItems)?.ToString()),
                        ("SalesTax", x => Math.Round(x.InvoiceLine.Tax, 2).ToString()),
                        ("Total", x => Math.Round(x.InvoiceLine.ExtendedAmount, 2).ToString()),
                        ("TaxAmt", x => ""),
                        ("DiscountTaxable", x => "N"),
                        ("LineServiceDate", x => x.Invoice.IssueDate?.ToString("d")),
                        ("LineItem", x => "Ticket Nbr: " + x.InvoiceLine.TicketNumber + " for " + x.InvoiceLine.Item?.Name),
                        ("LineItemDesc", x => x.InvoiceLine.Item?.Description),
                        ("LineItemIncomeAccount", x => x.InvoiceLine.Item?.IncomeAccount),
                        ("TicketNumber", x => x.InvoiceLine.TicketNumber),
                        ("TicketDate", x => x.InvoiceLine.Ticket?.TicketDateTimeUtc?.ConvertTimeZoneTo(timezone).Date.ToString("d")),
                        ("LoadAtName", x => x.InvoiceLine.Ticket?.LoadAt?.Name),
                        ("LoadAtStreetAddress", x => x.InvoiceLine.Ticket?.LoadAt?.StreetAddress),
                        ("LoadAtCity", x => x.InvoiceLine.Ticket?.LoadAt?.City),
                        ("LoadAtState", x => x.InvoiceLine.Ticket?.LoadAt?.State),
                        ("DeliverToName", x => x.InvoiceLine.Ticket?.DeliverTo?.Name),
                        ("DeliverAtStreetAddress", x => x.InvoiceLine.Ticket?.DeliverTo?.StreetAddress),
                        ("DeliverAtCity", x => x.InvoiceLine.Ticket?.DeliverTo?.City),
                        ("DeliverAtState", x => x.InvoiceLine.Ticket?.DeliverTo?.State),
                        ("DeliverAtZipCode", x => x.InvoiceLine.Ticket?.DeliverTo?.ZipCode),
                        ("JobNumber", x => x.InvoiceLine.JobNumber),
                        ("LineDesc", x => x.InvoiceLine.Description),
                        ("LineQty", x => x.InvoiceLine.Quantity.ToString()),
                        ("Type", x => x.InvoiceLine.FreightRate > 0 && x.InvoiceLine.MaterialRate > 0 ? "Freight and Material"
                            : x.InvoiceLine.FreightRate > 0 ? "Freight"
                            : x.InvoiceLine.MaterialRate > 0 ? "Material"
                            : ""),
                        ("FreightUnitPrice", x => x.InvoiceLine.FreightRate.ToString()),
                        ("MaterialUnitPrice", x => x.InvoiceLine.MaterialRate.ToString()),
                        ("LineUnitPrice", x => (x.InvoiceLine.FreightRate + x.InvoiceLine.MaterialRate).ToString()),
                        ("MaterialAmount", x => x.InvoiceLine.MaterialExtendedAmount.ToString()),
                        ("FreightAmount", x => x.InvoiceLine.FreightExtendedAmount.ToString()),
                        ("LineAmount", x => x.InvoiceLine.Subtotal.ToString()),
                        ("LineClass", x => ""),
                        ("LineTaxable", x => x.InvoiceLine.Tax > 0 ? "Y" : "N"),
                        ("Crew #", x => ""),
                        ("Truck", x => x.InvoiceLine.Ticket?.TruckCode),
                        ("Carrier", x => x.InvoiceLine.Ticket?.CarrierName),
                        ("LeaseHaulerRate", x => (x.InvoiceLine.Ticket?.CarrierId != null ? x.InvoiceLine.Ticket?.LeaseHaulerRate : 0)?.ToString()),
                        ("LeaseHaulerCost", x => (x.InvoiceLine.Ticket?.CarrierId != null ? x.InvoiceLine.Ticket?.LeaseHaulerCost : 0)?.ToString()),
                        (!separateItems || alwaysShowFreightAndMaterialOnSeparateLines ? "MaterialGL" : null, x => x.InvoiceLine.MaterialItem?.IncomeAccount),
                        (!separateItems || alwaysShowFreightAndMaterialOnSeparateLines ? "FreightGL" : null, x => x.InvoiceLine.FreightItem?.IncomeAccount),
                        (!separateItems || alwaysShowFreightAndMaterialOnSeparateLines ? "SalesTaxGL" : null, x => x.InvoiceLine.SalesTaxItem?.IncomeAccount),
                        (!separateItems || alwaysShowFreightAndMaterialOnSeparateLines ? "MaterialDescription" : null, x => x.InvoiceLine.MaterialItem?.Name)
                    );

                });
        }
    }
}
