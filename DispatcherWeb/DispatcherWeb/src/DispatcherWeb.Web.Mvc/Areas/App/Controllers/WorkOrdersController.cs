using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.IO.Extensions;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.WorkOrders;
using DispatcherWeb.WorkOrders.Dto;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_WorkOrders_View)]
    public class WorkOrdersController : DispatcherWebControllerBase
    {
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly IWorkOrderAppService _workOrderAppService;

        public WorkOrdersController(
            IAttachmentHelper attachmentHelper,
            IWorkOrderAppService workOrderAppService
        )
        {
            _attachmentHelper = attachmentHelper;
            _workOrderAppService = workOrderAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int? id)
        {
            var model = await _workOrderAppService.GetWorkOrderForEdit(new NullableIdDto(id));
            return View(model);
        }

        public async Task<PartialViewResult> CreateOrEditWorkOrderLineModal(int? id, int workOrderId)
        {
            var model = await _workOrderAppService.GetWorkOrderLineForEdit(new GetWorkOrderLineForEditInput(id, workOrderId));
            return PartialView("_CreateOrEditWorkOrderLineModal", model);
        }

        public async Task<IActionResult> UploadPicture()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.PaidFunctionality))
            {
                throw new ApplicationException(L("UpgradeToAccessThisFunctionality"));
            }

            var form = await Request.ReadFormAsync();
            var file = form.Files.Any() ? form.Files[0] : null;
            int workOrderId = int.Parse(form["id"].First());

            if (file != null)
            {
                byte[] fileBytes;
                await using (var fileStream = file.OpenReadStream())
                {
                    fileBytes = fileStream.GetAllBytes();
                }
                var fileId = await _attachmentHelper.UploadToAzureBlobAsync(fileBytes, workOrderId, file.ContentType, BlobContainerNames.WorkOrderPictures);
                var document = await _workOrderAppService.SavePicture(new WorkOrderPictureEditDto
                {
                    FileId = fileId,
                    WorkOrderId = workOrderId,
                    FileName = file.FileName,
                });

                return Json(new AjaxResponse(new
                {
                    id = document.Id,
                    workOrderId = document.WorkOrderId,
                    fileId = document.FileId,
                    fileName = document.FileName,
                }));
            }

            return Json(new AjaxResponse(false));

        }

        public async Task<PartialViewResult> GetPictureRow(int id)
        {
            var model = await _workOrderAppService.GetPictureEditDto(id);
            return PartialView("_Picture", model);
        }

        public async Task<IActionResult> DownloadPicture(int workOrderId, Guid fileId, string fileName)
        {
            var file = await _attachmentHelper.GetFromAzureBlobAsync($"{workOrderId}/{fileId}", BlobContainerNames.WorkOrderPictures);
            return File(file.Content, "application/octet-stream", fileName);
        }
    }
}
