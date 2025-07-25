using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.TempFiles;
using DispatcherWeb.Web.Areas.App.Models.Shared;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize]
    public class TempFileController : DispatcherWebControllerBase
    {
        private readonly ITempFileAppService _tempFileService;

        public TempFileController(
            ITempFileAppService tempFileService
        )
        {
            _tempFileService = tempFileService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_TempFiles)]
        public async Task<IActionResult> DownloadTempFile(int tempFileId)
        {
            try
            {
                var file = await _tempFileService.DownloadTempFile(tempFileId);
                return File(file.FileBytes, file.MimeType, file.FileName);
            }
            catch (UserFriendlyException ex)
            {
                return View("UserFriendlyException", new UserFriendlyExceptionViewModel
                {
                    Message = ex.Message,
                });
            }
        }
    }
}
