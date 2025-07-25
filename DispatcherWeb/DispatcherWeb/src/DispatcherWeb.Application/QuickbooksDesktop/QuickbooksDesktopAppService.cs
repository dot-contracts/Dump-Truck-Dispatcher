using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices;
using DispatcherWeb.Invoices.Dto;
using DispatcherWeb.Items;
using DispatcherWeb.Net.MimeTypes;
using DispatcherWeb.QuickbooksDesktop.Dto;
using DispatcherWeb.QuickbooksDesktop.Models;
using DispatcherWeb.QuickbooksOnline;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.QuickbooksDesktop
{
    [AbpAuthorize(AppPermissions.Pages_Invoices)]
    public class QuickbooksDesktopAppService : DispatcherWebAppServiceBase, IQuickbooksDesktopAppService
    {
        private const int CustomerNameMaxLength = 41;
        private const string Yes = "Y";
        private const string No = "N";
        private const string DateFormat = "MM/dd/yy"; //"yyyy-MM-dd"

        private readonly IInvoiceAppService _invoiceAppService;
        private readonly IRepository<Invoices.Invoice> _invoiceRepository;
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Invoices.InvoiceUploadBatch> _invoiceUploadBatchRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ITempFileCacheManager _tempFileCacheManager;

        public QuickbooksDesktopAppService(
            IInvoiceAppService invoiceAppService,
            IRepository<Invoices.Invoice> invoiceRepository,
            IRepository<Item> itemRepository,
            IRepository<Customer> customerRepository,
            IRepository<Invoices.InvoiceUploadBatch> invoiceUploadBatchRepository,
            IBinaryObjectManager binaryObjectManager,
            ITempFileCacheManager tempFileCacheManager
            )
        {
            _invoiceAppService = invoiceAppService;
            _invoiceRepository = invoiceRepository;
            _itemRepository = itemRepository;
            _customerRepository = customerRepository;
            _invoiceUploadBatchRepository = invoiceUploadBatchRepository;
            _binaryObjectManager = binaryObjectManager;
            _tempFileCacheManager = tempFileCacheManager;
        }

        public async Task<FileDto> ExportInvoicesToIIF(ExportInvoicesToIIFInput input)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var alwaysShowFreightAndMaterialOnSeparateLines = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles);

            InvoiceAppService.ValidateGetInvoicesInputForExport(input);

            var invoiceQuery = (await _invoiceRepository.GetQueryAsync())
                .Include(x => x.InvoiceLines)
                .WhereIf(!input.IncludeExportedInvoices, x => x.QuickbooksExportDateTime == null);

            invoiceQuery = InvoiceAppService.FilterInvoiceQuery(invoiceQuery, input);

            var invoicesToReorder = await invoiceQuery.ToListAsync();
            foreach (var invoice in invoicesToReorder)
            {
                await InvoiceAppService.ReorderInvoiceLines(invoice, SettingManager);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            var invoicesToUpload = await invoiceQuery
                .ToInvoiceToUploadList(await GetTimezone(), separateItems);

            try
            {
                await _invoiceAppService.ValidateInvoiceStatusChange(new ValidateInvoiceStatusChangeInput
                {
                    Ids = invoicesToUpload.Select(x => x.InvoiceId).ToArray(),
                    Status = InvoiceStatus.Sent,
                });
            }
            catch (UserFriendlyException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }

            var salesTaxItem = await (await _itemRepository.GetQueryAsync()).GetSalesTaxItem();
            invoicesToUpload.ForEach(x => x.InvoiceLines.ForEach(l => l.SalesTaxItem = salesTaxItem));

            var invoiceNumberPrefix = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);
            var taxCalculationType = (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType);

            invoicesToUpload.SplitMaterialAndFreightLines(separateItems, taxCalculationType, alwaysShowFreightAndMaterialOnSeparateLines);

            //foreach (var invoice in invoicesToUpload)
            //{
            //    invoice.RecalculateTotals(taxCalculationType);
            //}

            if (!invoicesToUpload.Any(x => x.InvoiceLines.Any()))
            {
                throw new UserFriendlyException("There are no new Invoices to export");
            }

            var uploadedInvoices = new List<Invoices.Invoice>();

            var DefaultIncomeAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountName); //"Income Services";
            var DefaultIncomeAccountType = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.DefaultIncomeAccountType); //"INC";
            var AccountsReceivableAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.AccountsReceivableAccountName); //"Accounts Receivable";
            var TaxAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.TaxAccountName); //"Sales Tax Payable";
            var TaxAgencyVendorName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.QuickbooksDesktop.TaxAgencyVendorName); //"TaxAgencyVendor";

            //var Memo = "System Generated Invoice";

            const string TaxInventoryItemName = "Add Tax";
            const string HasCleared = No;
            var todayFormatted = (await GetToday()).ToString(DateFormat);
            var timezone = await GetTimezone();


            var s = new StringBuilder();


            AccountRow.HeaderRow.AppendRow(s);
            new AccountRow
            {
                Name = AccountsReceivableAccountName,
                AccountType = AccountTypes.AccountsReceivable,
            }.AppendRow(s);

            new AccountRow
            {
                Name = DefaultIncomeAccountName,
                AccountType = DefaultIncomeAccountType, //AccountTypes.Income
            }.AppendRow(s);

            new AccountRow
            {
                Name = TaxAccountName,
                AccountType = AccountTypes.OtherCurrentLiability,
            }.AppendRow(s);

            VendorRow.HeaderRow.AppendRow(s);
            new VendorRow
            {
                Name = TaxAgencyVendorName,
                VendorType = VendorTypes.TaxAgency,
            }.AppendRow(s);

            var tooLongItemNames = new List<string>();

            ItemRow.HeaderRow.AppendRow(s);
            foreach (var itemGroup in invoicesToUpload
                .SelectMany(x => x.InvoiceLines)
                .Where(x => x.Item?.IsInQuickBooks == false)
                .GroupBy(x => x.Item.Name))
            {
                var invoiceLine = itemGroup.First();

                if (invoiceLine.Item.Type == ItemType.InventoryPart)
                {
                    continue; //Exclude "Inventory Part" until someone asks for it as an option.
                    //Inventory parts have more required fields, e.g. AssetAccount, CogsAccount, maybe Cost
                }

                var itemType = QuickbooksItemTypes.FromItemType(invoiceLine.Item.Type);
                if (itemType == null)
                {
                    continue;
                }

                if (invoiceLine.Item.Name?.Length > EntityStringFieldLengths.Item.ServiceInQuickBooks)
                {
                    tooLongItemNames.Add(invoiceLine.Item.Name);
                    continue;
                }

                if (invoiceLine.Quantity == 0)
                {
                    invoiceLine.Quantity = 1;
                }
                new ItemRow
                {
                    Name = invoiceLine.Item.Name,
                    ItemType = itemType, //ticket.ServiceHasMaterialPricing ? ItemTypes.NonInventoryPartItem : ItemTypes.ServiceItem,
                    Account = !invoiceLine.Item.IncomeAccount.IsNullOrEmpty() ? invoiceLine.Item.IncomeAccount : DefaultIncomeAccountName,
                    //AssetAccount = ASSETACCNT,
                    //CogsAccount = COGSACCNT,
                    Price = (invoiceLine.Subtotal / invoiceLine.Quantity).ToString(),
                    //Cost = Convert.ToString(invoiceLine.FreightExtendedAmount + invoiceLine.MaterialExtendedAmount),
                    Taxable = Yes,
                    //PaymentMethod = PAYMETH,
                    //TaxAgency = TAXVEND,
                    //TaxDistrict = TAXDIST,
                }.AppendRow(s);
            }

            if (tooLongItemNames.Any())
            {
                throw new UserFriendlyException("The following products/services exceed the limit of 31 characters: \r\n" + string.Join(", \r\n", tooLongItemNames));
            }

            foreach (var invoiceTax in invoicesToUpload.Where(x => x.TaxRate > 0).GroupBy(x => x.TaxRate))
            {
                new ItemRow
                {
                    Name = TaxInventoryItemName + " " + invoiceTax.Key + "%",
                    ItemType = QuickbooksItemTypes.SalesTax,
                    Account = TaxAccountName,
                    //AssetAccount
                    //CogsAccount
                    Price = invoiceTax.Key + "%",
                    Cost = "0",
                    Taxable = No,
                    TaxAgency = TaxAgencyVendorName,
                }.AppendRow(s);
            }


            foreach (var invoice in invoicesToUpload)
            {
                if (!invoice.InvoiceLines.Any())
                {
                    continue;
                }

                var customerName = RemoveRestrictedCharacters(invoice.Customer.Name).Truncate(CustomerNameMaxLength);
                if (!invoice.Customer.IsInQuickBooks)
                {
                    CustomerRow.HeaderRow.AppendRow(s);
                    new CustomerRow
                    {
                        Name = customerName,
                        Email = invoice.Customer.InvoiceEmail,
                        Taxable = Yes,
                    }.SetShippingAddress(invoice.Customer.ShippingAddress)
                    .SetBillingAddress(invoice.Customer.BillingAddress)
                    .AppendRow(s);
                }

                TransactionRow.HeaderRow.AppendRow(s);
                TransactionLineRow.HeaderRow.AppendRow(s);
                TransactionEndRow.HeaderRow.AppendRow(s);

                new TransactionRow
                {
                    TransactionId = invoice.InvoiceId.ToString(),
                    TransactionType = TransactionTypes.Invoice,
                    Date = invoice.IssueDate?.ToString(DateFormat) ?? todayFormatted,
                    DueDate = invoice.DueDate?.ToString(DateFormat),
                    Account = AccountsReceivableAccountName,
                    Name = customerName,
                    Amount = Convert.ToString(invoice.TotalAmount),
                    DocNumber = invoiceNumberPrefix + invoice.InvoiceId.ToString(),
                    InvoiceMemo = invoice.Message,
                    PoNumber = invoice.GetPoNumberOrJobNumber(),
                    HasCleared = HasCleared,
                    NameIsTaxable = Yes,
                    Terms = invoice.Terms.GetDisplayName(),
                }.SetAddress(customerName, invoice.BillingAddress)
                .AppendRow(s);

                foreach (var lineItem in invoice.InvoiceLines.OrderBy(x => x.LineNumber))
                {
                    if (lineItem.Quantity == 0)
                    {
                        lineItem.Quantity = 1;
                    }
                    new TransactionLineRow
                    {
                        TransactionLineId = lineItem.LineNumber.ToString(),
                        TransactionType = TransactionTypes.Invoice,
                        Date = invoice.IssueDate?.ToString(DateFormat) ?? todayFormatted,
                        ServiceDate = lineItem.DeliveryDateTime?.ToString(DateFormat),
                        Account = !string.IsNullOrEmpty(lineItem.Item?.IncomeAccount) ? lineItem.Item.IncomeAccount : DefaultIncomeAccountName,
                        Name = customerName,
                        Amount = (-1 * lineItem.Subtotal).ToString(),
                        DocNumber = invoice.InvoiceId.ToString(),
                        Memo = lineItem.DescriptionAndTicketWithTruck,
                        HasCleared = HasCleared,
                        Quantity = (-1 * lineItem.Quantity).ToString(),
                        Price = (lineItem.Subtotal / lineItem.Quantity).ToString(),
                        //Price = (-1 * (lineItem.Ticket?.OrderMaterialPrice ?? 0 + lineItem.Ticket?.OrderFreightPrice ?? 0)).ToString(),
                        Item = lineItem.Item?.Name,
                        Taxable = lineItem.Tax > 0 ? Yes : No,
                    }.AppendRow(s);
                }

                if (invoice.Tax > 0)
                {
                    new TransactionLineRow
                    {
                        TransactionLineId = (invoice.InvoiceLines.Max(x => x.LineNumber) + 1).ToString(),
                        TransactionType = TransactionTypes.Invoice,
                        Date = invoice.IssueDate?.ToString(DateFormat) ?? todayFormatted,
                        Account = TaxAccountName,
                        Name = TaxAgencyVendorName,
                        Amount = invoice.Tax.ToString(),
                        DocNumber = invoice.InvoiceId.ToString(),
                        HasCleared = HasCleared,
                        Price = invoice.TaxRate + "%",
                        Item = TaxInventoryItemName + " " + invoice.TaxRate + "%",
                        Taxable = No,
                        Extra = ExtraKeywords.InvoiceLine.AutoSalesTax,
                    }.AppendRow(s);
                }
                new TransactionEndRow().AppendRow(s);

                uploadedInvoices.Add(invoice.Invoice);
            }

            var iifContents = s.ToString();
            var iifBytes = Encoding.UTF8.GetBytes(iifContents);
            var filename = "Invoices";

            if (!input.IncludeExportedInvoices)
            {
                var invoiceUploadBatch = new Invoices.InvoiceUploadBatch { TenantId = await AbpSession.GetTenantIdAsync() };
                await _invoiceUploadBatchRepository.InsertAndGetIdAsync(invoiceUploadBatch);
                filename += $"Batch-{invoiceUploadBatch.Id}";

                foreach (var invoice in uploadedInvoices)
                {
                    invoice.QuickbooksExportDateTime = Clock.Now;
                    invoice.UploadBatchId = invoiceUploadBatch.Id;
                    invoice.Status = InvoiceStatus.Sent;
                }
                invoiceUploadBatch.FileGuid = await _binaryObjectManager.UploadByteArrayAsync(iifBytes, await AbpSession.GetTenantIdAsync());

                await CurrentUnitOfWork.SaveChangesAsync();

                var itemIds = invoicesToUpload
                    .SelectMany(x => x.InvoiceLines)
                    .Where(x => x.Item?.IsInQuickBooks == false)
                    .Select(x => x.Item.Id)
                    .Distinct()
                    .ToList();

                if (itemIds.Any())
                {
                    var items = await (await _itemRepository.GetQueryAsync()).Where(x => itemIds.Contains(x.Id)).ToListAsync();
                    items.ForEach(x => x.IsInQuickBooks = true);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                var customerIds = invoicesToUpload
                    .Where(x => !x.Customer.IsInQuickBooks)
                    .Select(x => x.Customer.Id)
                    .Distinct()
                    .ToList();

                if (customerIds.Any())
                {
                    var customers = await (await _customerRepository.GetQueryAsync()).Where(x => customerIds.Contains(x.Id)).ToListAsync();
                    customers.ForEach(x => x.IsInQuickBooks = true);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            return await _tempFileCacheManager.StoreTempFileAsync(new FileBytesDto
            {
                FileBytes = iifBytes,
                FileName = filename + ".iif",
                MimeType = MimeTypeNames.TextPlain,
            });
        }

        private string RemoveRestrictedCharacters(string val)
        {
            return val?.Replace(":", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
        }
    }
}
