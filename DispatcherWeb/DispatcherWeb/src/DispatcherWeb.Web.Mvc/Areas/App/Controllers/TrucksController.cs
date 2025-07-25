using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Extensions;
using Abp.IO.Extensions;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Features;
using DispatcherWeb.Images;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.Web.Areas.App.Models.Trucks;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("app")]
    [AbpMvcAuthorize(AppPermissions.Pages_Trucks)]
    public class TrucksController : DispatcherWebControllerBase
    {
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly ITruckAppService _truckAppService;
        private readonly ITruckTelematicsAppService _truckTelematicsAppService;

        public TrucksController(
            IAttachmentHelper attachmentHelper,
            ITruckAppService truckAppService,
            ITruckTelematicsAppService truckTelematicsAppService
        )
        {
            _attachmentHelper = attachmentHelper;
            _truckAppService = truckAppService;
            _truckTelematicsAppService = truckTelematicsAppService;
        }

        public async Task<IActionResult> Index(
            bool? filterIsOutOfService,
            bool? filterPlatesExpiringThisMonth,
            int? filterOfficeId,
            string filterOfficeName,
            int? filterStatus
        )
        {
            var model = new TruckViewListModel() { Filter = new GetTrucksInput() };
            model.Filter.IsOutOfService = filterIsOutOfService;
            model.Filter.PlatesExpiringThisMonth = filterPlatesExpiringThisMonth ?? false;
            model.Filter.OfficeId = filterOfficeId;
            model.OfficeName = filterOfficeName;
            model.Filter.Status = filterStatus.HasValue ? (FilterActiveStatus)filterStatus.Value : FilterActiveStatus.All;
            model.IsGpsIntegrationConfigured = await _truckTelematicsAppService.IsGpsIntegrationConfigured();
            model.IsDtdTrackerConfigured = await _truckTelematicsAppService.IsDtdTrackerConfigured();
            model.IsIntelliShiftConfigured = await _truckTelematicsAppService.IsIntelliShiftConfigured();
            return View(model);
        }

        public async Task<PartialViewResult> CreateOrEditModal(GetTruckForEditInput input)
        {
            var model = await _truckAppService.GetTruckForEdit(input);
            model.IsGpsIntegrationConfigured = await _truckTelematicsAppService.IsGpsIntegrationConfigured();
            return PartialView("_CreateOrEditModal", model);
        }

        [Modal]
        public PartialViewResult AddOutOfServiceReasonModal(int truckId, DateTime date)
        {
            var model = new SetTruckIsOutOfServiceInput() { TruckId = truckId, Date = date };
            return PartialView("_AddOutOfServiceReasonModal", model);
        }

        public async Task<IActionResult> UploadFile()
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.PaidFunctionality))
            {
                throw new ApplicationException(L("UpgradeToAccessThisFunctionality"));
            }

            var form = await Request.ReadFormAsync();
            var file = form.Files.Any() ? form.Files[0] : null;
            var truckId = int.Parse(form["id"].First());

            if (file != null)
            {
                var fileType = GetFileType(file.FileName);
                byte[] fileBytes;
                await using (var fileStream = file.OpenReadStream())
                {
                    fileBytes = fileStream.GetAllBytes();
                }
                var fileId = await _attachmentHelper.UploadToAzureBlobAsync(fileBytes, truckId, file.ContentType, BlobContainerNames.TruckFiles);

                Guid? thumbnailId = null;
                if (Utilities.IsImageFileType(fileType))
                {
                    await using (var fileStream = file.OpenReadStream())
                    {
                        thumbnailId = await CreateThumbnailAsync(Image.FromStream(fileStream), truckId, file);
                    }
                }

                var truckFile = await _truckAppService.SaveFile(new TruckFileEditDto
                {
                    FileId = fileId,
                    ThumbnailId = thumbnailId,
                    TruckId = truckId,
                    FileName = file.FileName.Truncate(500),
                    Title = file.FileName.Truncate(50),
                    FileType = fileType,
                });

                return Json(new AjaxResponse(new
                {
                    id = truckFile.Id,
                    vehicleServiceId = truckFile.TruckId,
                    fileId = truckFile.FileId,
                    title = truckFile.Title,
                }));
            }

            return Json(new AjaxResponse(false));

        }

        private async Task<Guid> CreateThumbnailAsync(Image fileImage, int truckId, IFormFile file)
        {
            Image thumbnailImage = ImageHelper.ResizePreservingRatio(fileImage, AppConsts.TruckFileThumbnailSize, AppConsts.TruckFileThumbnailSize);
            byte[] fileBytes;
            using (var saveStream = new MemoryStream())
            {
                thumbnailImage.Save(saveStream, fileImage.RawFormat);
                saveStream.Position = 0;
                fileBytes = saveStream.GetAllBytes();
            }

            Guid thumbnailId = await _attachmentHelper.UploadToAzureBlobAsync(fileBytes, truckId, file.ContentType, BlobContainerNames.TruckFiles);
            return thumbnailId;
        }

        private FileType GetFileType(string fileName)
        {
            string fileExtension = fileName.Split('.').Last().ToLower();
            switch (fileExtension)
            {
                case "bmp":
                    return FileType.Bmp;
                case "jpg":
                case "jpeg":
                    return FileType.Jpg;
                case "gif":
                    return FileType.Gif;
                case "png":
                    return FileType.Png;

                case "doc":
                    return FileType.Doc;
                case "pdf":
                    return FileType.Pdf;

                default:
                    return FileType.Unknown;
            }
        }

        public async Task<IActionResult> DownloadFile(int truckId, Guid fileId, string fileName)
        {
            var file = await _attachmentHelper.GetFromAzureBlobAsync($"{truckId}/{fileId}", BlobContainerNames.TruckFiles);
            return File(file.Content, file.ContentType, fileName);
        }

        public async Task<PartialViewResult> GetFileRow(int id)
        {
            var model = await _truckAppService.GetTruckFileEditDto(id);
            return PartialView("_FileRow", model);
        }


    }

}
