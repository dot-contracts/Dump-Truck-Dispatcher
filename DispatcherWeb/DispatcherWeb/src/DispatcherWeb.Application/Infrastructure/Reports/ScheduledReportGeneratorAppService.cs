using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using DispatcherWeb.Application.Infrastructure.Utilities;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Reports.Dto;
using DispatcherWeb.Trucks.OutOfServiceTrucksReport;

namespace DispatcherWeb.Infrastructure.Reports
{
    [RemoteService(false)]
    public class ScheduledReportGeneratorAppService : DispatcherWebAppServiceBase, IScheduledReportGeneratorAppService
    {
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly IocManager _iocManager;
        private readonly IEmailSender _emailSender;
        private CustomSession _customSession;

        public ScheduledReportGeneratorAppService(
            IAttachmentHelper attachmentHelper,
            IocManager iocManager,
            IEmailSender emailSender
        )
        {
            _attachmentHelper = attachmentHelper;
            _iocManager = iocManager;
            _emailSender = emailSender;
        }

        [UnitOfWork]
        public virtual async Task GenerateReport(ScheduledReportGeneratorInput scheduledReportGeneratorInput)
        {
            Logger.Info($"Start generating scheduled report {scheduledReportGeneratorInput.ReportType}");
            _customSession = scheduledReportGeneratorInput.CustomSession;
            using (AbpSession.Use(_customSession.TenantId, _customSession.UserId))
            using (CurrentUnitOfWork.SetTenantId(_customSession.TenantId))
            {
                if (!await PermissionChecker.IsGrantedAsync(await AbpSession.ToUserIdentifierAsync(), scheduledReportGeneratorInput.ReportType.GetPermissionName()))
                {
                    Logger.Error($"The user don't have the {scheduledReportGeneratorInput.ReportType.GetPermissionName()} permission for the {scheduledReportGeneratorInput.ReportType} report.");
                    return;
                }

                ReportAppServiceBase<EmptyInput> reportAppService = null;
                try
                {
                    reportAppService = GetReportService(scheduledReportGeneratorInput.ReportType);
                    var input = new EmptyInput();
                    FileDto file;
                    if (scheduledReportGeneratorInput.ReportFormat == ReportFormat.Pdf)
                    {
                        file = await reportAppService.CreatePdf(input);
                    }
                    else
                    {
                        file = await reportAppService.CreateCsv(input);
                    }
                    await SendEmailsAsync(scheduledReportGeneratorInput.EmailAddresses, file, scheduledReportGeneratorInput.ReportType);

                }
                catch (Exception e)
                {
                    Logger.Error($"Error when generating report: {e}\n input: {scheduledReportGeneratorInput}");
                }
                finally
                {
                    _iocManager.Release(reportAppService);
                }
            }

        }

        private async Task SendEmailsAsync(string[] emailAddresses, FileDto file, ReportType reportType)
        {
            var fileBytes = await _attachmentHelper.GetFromAzureBlobAsync($"{_customSession.UserId}/{file.FileToken}", BlobContainerNames.ReportFiles);
            string from = await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress);

            await using (Stream stream = new MemoryStream(fileBytes.Content))
            {
                var attachment = CreateAttachment(stream, file.FileName, file.FileType);
                foreach (var emailAddress in emailAddresses)
                {
                    await SendEmailAsync(from, emailAddress, attachment, reportType);
                }
            }
        }

        private async Task SendEmailAsync(string from, string to, Attachment attachment, ReportType reportType)
        {
            try
            {
                var message = new MailMessage(from, to, $"Generated report {reportType}", "");
                message.Attachments.Add(attachment);
                await _emailSender.SendAsync(message);

            }
            catch (Exception e)
            {
                Logger.Error($"Error sending email from {from} to {to}: {e}");
            }
        }

        private Attachment CreateAttachment(Stream stream, string fileName, string fileType)
        {
            return new Attachment(stream, fileName, fileType);
        }

        private ReportAppServiceBase<EmptyInput> GetReportService(ReportType reportType)
        {
            switch (reportType)
            {
                case ReportType.OutOfServiceTrucks:
                    var report = _iocManager.Resolve<OutOfServiceTrucksReportAppService>();
                    report.SetCustomSession(_customSession);
                    return report;

                default:
                    throw new ApplicationException($"Unsupported report type: {reportType}");
            }
        }

    }
}
