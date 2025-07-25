using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices;
using DispatcherWeb.Invoices.Dto;
using DispatcherWeb.Items;
using DispatcherWeb.QuickbooksOnline;
using DispatcherWeb.QuickbooksOnline.Dto;
using DispatcherWeb.QuickbooksOnlineExport.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.QuickbooksOnlineExport
{
    [AbpAuthorize(AppPermissions.Pages_Invoices)]
    public class QuickbooksOnlineExportAppService : DispatcherWebAppServiceBase, IQuickbooksOnlineExportAppService
    {
        private readonly IInvoiceAppService _invoiceAppService;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<InvoiceUploadBatch> _invoiceUploadBatchRepository;
        private readonly IRepository<Item> _itemRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IQuickbooksOnlineCsvExporter _quickbooksOnlineCsvExporter;
        private readonly ITransactionProCsvExporter _transactionProCsvExporter;
        private readonly ISbtCsvExporter _sbtCsvExporter;
        private readonly ISageCsvExporter _sageCsvExporter;
        private readonly IHollisCsvExporter _hollisCsvExporter;
        private readonly IJAndJCsvExporter _jandjCsvExporter;

        public QuickbooksOnlineExportAppService(
            IInvoiceAppService invoiceAppService,
            IRepository<Invoice> invoiceRepository,
            IRepository<InvoiceUploadBatch> invoiceUploadBatchRepository,
            IRepository<Item> itemRepository,
            IBinaryObjectManager binaryObjectManager,
            ITempFileCacheManager tempFileCacheManager,
            IQuickbooksOnlineCsvExporter quickbooksOnlineCsvExporter,
            ITransactionProCsvExporter transactionProCsvExporter,
            ISbtCsvExporter sbtCsvExporter,
            ISageCsvExporter sageCsvExporter,
            IHollisCsvExporter hollisCsvExporter,
            IJAndJCsvExporter jandjCsvExporter
            )
        {
            _invoiceAppService = invoiceAppService;
            _invoiceRepository = invoiceRepository;
            _invoiceUploadBatchRepository = invoiceUploadBatchRepository;
            _itemRepository = itemRepository;
            _binaryObjectManager = binaryObjectManager;
            _tempFileCacheManager = tempFileCacheManager;
            _quickbooksOnlineCsvExporter = quickbooksOnlineCsvExporter;
            _transactionProCsvExporter = transactionProCsvExporter;
            _sbtCsvExporter = sbtCsvExporter;
            _sageCsvExporter = sageCsvExporter;
            _hollisCsvExporter = hollisCsvExporter;
            _jandjCsvExporter = jandjCsvExporter;
        }

        public async Task<FileDto> ExportInvoicesToCsv(ExportInvoicesToCsvInput input)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            InvoiceAppService.ValidateGetInvoicesInputForExport(input);

            var invoiceQuery = (await _invoiceRepository.GetQueryAsync())
                .Where(x => x.InvoiceLines.Any())
                .WhereIf(!input.IncludeExportedInvoices, x => x.QuickbooksExportDateTime == null);

            invoiceQuery = InvoiceAppService.FilterInvoiceQuery(invoiceQuery, input);

            var invoicesToUpload = await invoiceQuery
                .ToInvoiceToUploadList(await GetTimezone(), separateItems);

            var salesTaxItem = await (await _itemRepository.GetQueryAsync()).GetSalesTaxItem();
            invoicesToUpload.ForEach(x => x.InvoiceLines.ForEach(l => l.SalesTaxItem = salesTaxItem));

            await SplitLinesIfNeededAsync(invoicesToUpload);

            if (!invoicesToUpload.Any(x => x.InvoiceLines.Any()))
            {
                throw new UserFriendlyException("There are no new Invoices to export");
            }

            foreach (var invoice in invoicesToUpload)
            {
                foreach (var invoiceLine in invoice.InvoiceLines)
                {
                    if (invoiceLine.Quantity == 0)
                    {
                        invoiceLine.Quantity = 1;
                    }

                    if (invoice.DueDate == null)
                    {
                        throw new UserFriendlyException($"Invoice #{invoice.InvoiceId} doesn't have Due Date specified. Please update the invoice and try again");
                    }
                }
            }

            InvoiceUploadBatch invoiceUploadBatch = null;
            var filename = "Invoices";

            if (!input.IncludeExportedInvoices)
            {
                await _invoiceAppService.ValidateInvoiceStatusChange(new ValidateInvoiceStatusChangeInput
                {
                    Ids = invoicesToUpload.Select(x => x.InvoiceId).ToArray(),
                    Status = InvoiceStatus.Sent,
                });

                invoiceUploadBatch = new InvoiceUploadBatch { TenantId = await AbpSession.GetTenantIdAsync() };
                await _invoiceUploadBatchRepository.InsertAndGetIdAsync(invoiceUploadBatch);

                filename += $"Batch-{invoiceUploadBatch.Id}";
            }

            var csvExporter = await GetCsvExporterAsync();

            var result = await csvExporter.ExportToFileAsync(invoicesToUpload, filename);

            if (!input.IncludeExportedInvoices)
            {
                foreach (var invoice in invoicesToUpload)
                {
                    invoice.Invoice.QuickbooksExportDateTime = Clock.Now;
                    invoice.Invoice.UploadBatchId = invoiceUploadBatch!.Id;
                    invoice.Invoice.Status = InvoiceStatus.Sent;
                }
                var fileBytes = await _tempFileCacheManager.GetFileAsync(result.FileToken);
                invoiceUploadBatch!.FileGuid = await _binaryObjectManager.UploadByteArrayAsync(fileBytes, await AbpSession.GetTenantIdAsync());
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }

        private async Task SplitLinesIfNeededAsync(List<InvoiceToUploadDto<Invoice>> invoicesToUpload)
        {
            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var alwaysShowFreightAndMaterialOnSeparateLines = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AlwaysShowFreightAndMaterialOnSeparateLinesInExportFiles);
            var taxCalculationType = (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType);

            invoicesToUpload.SplitMaterialAndFreightLines(separateItems, taxCalculationType, alwaysShowFreightAndMaterialOnSeparateLines);
        }

        private async Task<IQuickbooksOnlineCsvExporterBase> GetCsvExporterAsync()
        {
            var quickbooksIntegrationKind = (QuickbooksIntegrationKind)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.Quickbooks.IntegrationKind);
            switch (quickbooksIntegrationKind)
            {
                case QuickbooksIntegrationKind.QboExport:
                    return _quickbooksOnlineCsvExporter;
                case QuickbooksIntegrationKind.TransactionProExport:
                    return _transactionProCsvExporter;
                case QuickbooksIntegrationKind.SbtCsvExport:
                    return _sbtCsvExporter;
                case QuickbooksIntegrationKind.SageExport:
                    return _sageCsvExporter;
                case QuickbooksIntegrationKind.HollisExport:
                    return _hollisCsvExporter;
                case QuickbooksIntegrationKind.JAndJExport:
                    return _jandjCsvExporter;
                default:
                    throw new UserFriendlyException("Quickbooks Integration Kind is not supported");
            }
        }
    }
}
