using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.Timing;
using CsvHelper;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Reports.Dto;
using DispatcherWeb.Net.MimeTypes;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;

namespace DispatcherWeb.Infrastructure.Reports
{
    public abstract class ReportAppServiceBase<TInput> : DispatcherWebAppServiceBase where TInput : class
    {
        private readonly IAttachmentHelper _attachmentHelper;
        private CustomSession _customSession;

        protected ReportAppServiceBase(
            IAttachmentHelper attachmentHelper
        )
        {
            _attachmentHelper = attachmentHelper;
        }

        public async Task<CustomSession> GetCustomSessionAsync()
        {
            if (_customSession == null)
            {
                var tenantId = await AbpSession.GetTenantIdOrNullAsync();
                if (tenantId == null || AbpSession.UserId == null)
                {
                    throw new ApplicationException("CustomSession must be set when running in a separate process!");
                }
                _customSession = new CustomSession(tenantId.Value, AbpSession.UserId.Value);
            }

            return _customSession;
        }

        public void SetCustomSession(CustomSession customSession)
        {
            _customSession = customSession;
        }

        [AbpAuthorize]
        public async Task<FileDto> CreatePdf(TInput input)
        {
            await CheckPermissions();
            var file = new FileDto(await GetReportFilename("pdf", input), MimeTypeNames.ApplicationPdf);

            Document document = new Document();
            document.DefineStyles();
            PdfReport report = new PdfReport(document, (await GetLocalDateTimeNow()).ToString("g"));
            InitPdfReport(report);

            if (await CreatePdfReport(report, input))
            {
                using (var stream = new MemoryStream())
                {
                    var pdfRenderer = new PdfDocumentRenderer(false) { Document = document };
                    pdfRenderer.RenderDocument();
                    pdfRenderer.PdfDocument.Save(stream, false);
                    var fileId = await _attachmentHelper.UploadToAzureBlobAsync(stream, (await GetCustomSessionAsync()).UserId, file.FileType, BlobContainerNames.ReportFiles);
                    file.FileToken = fileId.ToString();
                }
            }
            else
            {
                file.FileName = "";
            }

            return file;
        }

        [AbpAuthorize]
        public async Task<FileDto> CreateCsv(TInput input)
        {
            await CheckPermissions();
            var file = new FileDto(await GetReportFilename("csv", input), MimeTypeNames.TextCsv);
            using (var stream = new MemoryStream())
            await using (var writer = new StreamWriter(stream, Encoding.UTF8))
            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                var report = new CsvReport(csv);
                if (await CreateCsvReport(report, input))
                {
                    await writer.FlushAsync();
                    stream.Seek(0, SeekOrigin.Begin);
                    var fileId = await _attachmentHelper.UploadToAzureBlobAsync(stream, (await GetCustomSessionAsync()).UserId, file.FileType, BlobContainerNames.ReportFiles);
                    file.FileToken = fileId.ToString();
                }
                else
                {
                    file.FileName = "";
                }

            }
            return file;
        }

        private async Task CheckPermissions()
        {
            if (!await PermissionChecker.IsGrantedAsync(await AbpSession.ToUserIdentifierAsync(), ReportPermission))
            {
                throw new AbpAuthorizationException($"The user don't have the {ReportPermission} permission for this report.");
            }
        }
        protected abstract string ReportPermission { get; }

        protected async Task<DateTime> GetLocalDateTimeNow()
        {
            var customSession = await GetCustomSessionAsync();
            var timezone = await GetTimezone(customSession.TenantId, customSession.UserId);
            return Clock.Now.ConvertTimeZoneTo(timezone);
        }

        protected virtual async Task<string> GetReportFilename(string extension, TInput input)
        {
            return $"{ReportFileName}_{await GetLocalDateTimeNow():yyyy_MM_dd}.{extension}".SanitizeFilename();
        }

        protected abstract string ReportFileName { get; }

        protected abstract void InitPdfReport(PdfReport report);
        protected abstract Task<bool> CreatePdfReport(PdfReport report, TInput input);
        protected abstract Task<bool> CreateCsvReport(CsvReport report, TInput input);

    }
}
