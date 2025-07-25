using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Net.MimeTypes;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Controllers
{
    public class DownloadReportFileController : AbpController
    {
        private readonly IAttachmentHelper _attachmentHelper;

        public DownloadReportFileController(
            IAttachmentHelper attachmentHelper
        )
        {
            _attachmentHelper = attachmentHelper;
        }

        [AbpMvcAuthorize]
        [DisableAuditing]
        public async Task<ActionResult> Index(FileDto file)
        {
            var fileBytes = await _attachmentHelper.GetFromAzureBlobAsync($"{AbpSession.UserId}/{file.FileToken}", BlobContainerNames.ReportFiles);
            if (fileBytes.Content.Length == 0)
            {
                return NotFound();
            }
            if (file.FileType == MimeTypeNames.ApplicationPdf)
            {
                Response.Headers["Content-Disposition"] = "inline; filename=" + file.FileName;
                return File(fileBytes.Content, file.FileType);
            }
            return File(fileBytes.Content, file.FileType, file.FileName);
        }

    }
}
