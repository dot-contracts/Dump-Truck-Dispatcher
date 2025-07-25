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
using DispatcherWeb.VehicleServices;
using DispatcherWeb.VehicleServices.Dto;
using DispatcherWeb.Web.Areas.App.Models.VehicleServices;
using DispatcherWeb.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_VehicleService_View)]
    public class VehicleServicesController : DispatcherWebControllerBase
    {
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly IVehicleServiceAppService _vehicleServiceAppService;

        public VehicleServicesController(
            IAttachmentHelper attachmentHelper,
            IVehicleServiceAppService vehicleServiceAppService
        )
        {
            _attachmentHelper = attachmentHelper;
            _vehicleServiceAppService = vehicleServiceAppService;
        }

        public IActionResult Index()
        {
            var viewModel = new VehicleServiceListViewModel();
            return View(viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_VehicleService_Edit)]
        public async Task<PartialViewResult> CreateOrEditVehicleServiceModal(int? id)
        {
            var dto = await _vehicleServiceAppService.GetForEdit(new NullableIdDto(id));
            var viewModel = CreateOrEditVehicleServiceModalViewModel.CreateFromDto(dto);

            return PartialView("_CreateOrEditVehicleServiceModal", viewModel);
        }

        public async Task<IActionResult> UploadDocument()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.PaidFunctionality))
            {
                throw new ApplicationException(L("UpgradeToAccessThisFunctionality"));
            }

            var form = await Request.ReadFormAsync();
            var file = form.Files.Any() ? form.Files[0] : null;
            var vehicleServiceId = int.Parse(form["id"].First());

            if (file != null)
            {
                byte[] fileBytes;
                await using (var fileStream = file.OpenReadStream())
                {
                    fileBytes = fileStream.GetAllBytes();
                }
                var fileId = await _attachmentHelper.UploadToAzureBlobAsync(fileBytes, vehicleServiceId, file.ContentType, BlobContainerNames.VehicleServiceDocuments);
                var document = await _vehicleServiceAppService.SaveDocument(new VehicleServiceDocumentEditDto
                {
                    FileId = fileId,
                    VehicleServiceId = vehicleServiceId,
                    Name = file.FileName.Split('.').FirstOrDefault(),
                });

                return Json(new AjaxResponse(new
                {
                    id = document.Id,
                    vehicleServiceId = document.VehicleServiceId,
                    fileId = document.FileId,
                    name = document.Name,
                    description = document.Description,
                }));
            }

            return Json(new AjaxResponse(false));

        }

        public async Task<IActionResult> DownloadDocument(int vehicleServiceId, Guid fileId, string fileName)
        {
            var file = await _attachmentHelper.GetFromAzureBlobAsync($"{vehicleServiceId}/{fileId}", BlobContainerNames.VehicleServiceDocuments);
            return File(file.Content, "application/octet-stream", fileName + ".pdf");
        }

    }
}
