using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using DispatcherWeb.Configuration;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.QuickbooksOnline.Dto;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.QuickbooksOnlineExport
{
    public class SbtCsvExporter : CsvExporterBase, ISbtCsvExporter
    {
        private readonly ISettingManager _settingManager;

        public SbtCsvExporter(
            ISettingManager settingManager,
            ITempFileCacheManager tempFileCacheManager
            ) : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
        }

        public async Task<FileDto> ExportToFileAsync<T>(List<InvoiceToUploadDto<T>> invoiceList, string filename)
        {
            var invoiceNumberPrefix = await _settingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);

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
                        ("Customer", x => x.Invoice.Customer.Name),
                        ("CustomerId", x => x.Invoice.Customer.Id.ToString()),
                        ("AccountNbr", x => x.Invoice.Customer.AccountNumber),
                        ("TxnDate", x => x.Invoice.IssueDate?.ToString("d")),
                        ("DueDate", x => x.Invoice.DueDate?.ToString("d")),
                        ("PONumber", x => x.Invoice.PONumber),
                        ("ShipDate", x => ""),
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
                        ("ToBePrinted", x => "N"),
                        ("ToBeEmailed", x => "N"),
                        ("AllowIPNPayment", x => "N"),
                        ("AllowOnlineCreditCardPayment", x => "N"),
                        ("AllowOnlineACHPayment", x => "N"),
                        ("ShipAmt", x => ""),
                        ("ShipItem", x => ""),
                        ("DiscountAmt", x => ""),
                        ("DiscountRate", x => ""),
                        ("TaxRate", x => ""),
                        ("TaxAmt", x => ""),
                        ("DiscountTaxable", x => "N"),
                        ("LineServiceDate", x => x.Invoice.IssueDate?.ToString("d")),
                        ("LineItem", x => "Ticket Nbr: " + x.InvoiceLine.TicketNumber + " for " + x.InvoiceLine.Item?.Name),
                        ("TicketNumber", x => x.InvoiceLine.TicketNumber),
                        ("LoadAtName", x => x.InvoiceLine.Ticket?.LoadAt?.Name),
                        ("LoadAtStreetAddress", x => x.InvoiceLine.Ticket?.LoadAt?.StreetAddress),
                        ("LoadAtCity", x => x.InvoiceLine.Ticket?.LoadAt?.City),
                        ("LoadAtState", x => x.InvoiceLine.Ticket?.LoadAt?.State),
                        ("DeliverToName", x => x.InvoiceLine.Ticket?.DeliverTo?.Name),
                        ("DeliverAtStreetAddress", x => x.InvoiceLine.Ticket?.DeliverTo?.StreetAddress),
                        ("DeliverAtCity", x => x.InvoiceLine.Ticket?.DeliverTo?.City),
                        ("DeliverAtState", x => x.InvoiceLine.Ticket?.DeliverTo?.State),
                        ("JobNumber", x => x.InvoiceLine.JobNumber),
                        ("FreightUOM", x => x.InvoiceLine.Ticket?.GetAmountTypeToUse().useFreight == true ? x.InvoiceLine.Ticket.TicketUomName : ""),
                        ("MaterialUOM", x => x.InvoiceLine.Ticket?.GetAmountTypeToUse().useMaterial == true ? x.InvoiceLine.Ticket.TicketMaterialUomName ?? x.InvoiceLine.Ticket.TicketUomName : ""),
                        ("LineDesc", x => x.InvoiceLine.Ticket?.Designation?.FreightAndMaterial() == true
                                && x.InvoiceLine.IsSplitFreightLine == true
                            ? "Freight"
                            : x.InvoiceLine.Item?.Name),
                        ("LineQty", x => x.InvoiceLine.Quantity.ToString()),
                        ("FreightUnitPrice", x => x.InvoiceLine.FreightRate.ToString()),
                        ("MaterialUnitPrice", x => x.InvoiceLine.MaterialRate.ToString()),
                        ("LineUnitPrice", x => (x.InvoiceLine.FreightRate + x.InvoiceLine.MaterialRate).ToString()),
                        ("MaterialAmount", x => x.InvoiceLine.MaterialExtendedAmount.ToString()),
                        ("FreightAmount", x => x.InvoiceLine.FreightExtendedAmount.ToString()),
                        ("LineAmount", x => x.InvoiceLine.Subtotal.ToString()),
                        ("LineClass", x => ""),
                        ("LineTaxable", x => x.InvoiceLine.IsFreightTaxable == true || x.InvoiceLine.IsMaterialTaxable == true ? "Y" : "N"),
                        ("Crew #", x => "")
                    );

                });
        }
    }
}
