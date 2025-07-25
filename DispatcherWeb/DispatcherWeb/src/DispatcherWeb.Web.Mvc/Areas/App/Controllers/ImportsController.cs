using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Timing;
using Abp.Web.Models;
using DispatcherWeb.Authorization;
using DispatcherWeb.Features;
using DispatcherWeb.Imports;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.SecureFiles;
using DispatcherWeb.Web.Areas.App.Models.Imports;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize(AppPermissions.Pages_Imports)]
    public class ImportsController : DispatcherWebControllerBase
    {
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly ISecureFileBlobService _secureFileBlobService;
        private readonly ISecureFilesAppService _secureFilesService;

        public ImportsController(
            IAttachmentHelper attachmentHelper,
            ISecureFileBlobService secureFileBlobService,
            ISecureFilesAppService secureFilesService
            )
        {
            _attachmentHelper = attachmentHelper;
            _secureFileBlobService = secureFileBlobService;
            _secureFilesService = secureFilesService;
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_FuelUsage)]
        [Route("app/ImportFuel")]
        public IActionResult FuelUsage()
        {
            return View();
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_VehicleUsage)]
        [Route("app/ImportVehicle")]
        public IActionResult VehicleUsage()
        {
            return View();
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Customers)]
        [Route("app/ImportCustomers")]
        public virtual async Task<IActionResult> Customers()
        {
            await FeatureChecker.IsEnabledAsync(AppFeatures.QuickbooksImportFeature);
            return View();
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Trucks)]
        [Route("app/ImportTrucks")]
        public virtual async Task<IActionResult> Trucks()
        {
            await FeatureChecker.IsEnabledAsync(AppFeatures.QuickbooksImportFeature);
            return View();
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Vendors)]
        [Route("app/ImportVendors")]
        public virtual async Task<IActionResult> Vendors()
        {
            await FeatureChecker.IsEnabledAsync(AppFeatures.QuickbooksImportFeature);
            return View();
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Items)]
        [Route("app/ImportServices")]
        public virtual async Task<IActionResult> Items()
        {
            await FeatureChecker.IsEnabledAsync(AppFeatures.QuickbooksImportFeature);
            return View();
        }

        [HttpGet]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Employees)]
        [Route("app/ImportEmployees")]
        public virtual async Task<IActionResult> Employees()
        {
            await FeatureChecker.IsEnabledAsync(AppFeatures.QuickbooksImportFeature);
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(
            AppPermissions.Pages_Imports_Tickets_LuckStoneEarnings,
            AppPermissions.Pages_Imports_Tickets_TruxEarnings,
            AppPermissions.Pages_Imports_Tickets_IronSheepdogEarnings)]
        public IActionResult ImportTicketsModal()
        {
            return PartialView("_ImportTicketsModal");
        }

        public PartialViewResult CancelModal()
        {
            return PartialView("_CancelModal");
        }

        [HttpGet]
        [Route("app/ImportResults/{id}/{fileName}")]
        public async Task<IActionResult> ImportResult(
            Guid id,
            string fileName
        )
        {
            string blobName = $"{id}/{fileName}";
            string validationResultJsonString = await _secureFileBlobService.GetChildBlobAsync(blobName, SecureFileChildFileNames.ImportResult);
            var validationResult = Utility.Deserialize<ImportResultDto>(validationResultJsonString);

            return View(validationResult);
        }

        [Modal]
        public async Task<PartialViewResult> ImportMappingModal(
            Guid id,
            string fileName,
            ImportType importType
        )
        {
            var model = await GetImportMappingViewModelAsync(id, fileName, importType);

            return PartialView("_ImportMappingModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> ImportJacobusEnergyModal(
            Guid id,
            string fileName,
            ImportType importType
        )
        {
            var model = await GetImportMappingViewModelAsync(id, fileName, importType);

            return PartialView("_ImportJacobusEnergyModal", model);
        }

        [Modal]
        public async Task<PartialViewResult> ImportWithNoMappingModal(
            Guid id,
            string fileName,
            ImportType importType
        )
        {
            var model = await GetImportMappingViewModelAsync(id, fileName, importType);

            return PartialView("_ImportWithNoMappingModal", model);
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_VehicleUsage)]
        public IActionResult ImportVehicleModal()
        {
            ViewBag.ImportType = ImportType.VehicleUsage;
            return PartialView("_ImportVehicleModal");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_FuelUsage)]
        public IActionResult ImportFuelModal()
        {
            ViewBag.ImportType = ImportType.FuelUsage;
            return PartialView("_ImportFuelModal");
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_Imports_Trucks)]
        public IActionResult ImportTrucksModal()
        {
            ViewBag.ImportType = ImportType.Trucks;
            return PartialView("_ImportTrucksModal");
        }

        private async Task<ImportMappingViewModel> GetImportMappingViewModelAsync(Guid id, string fileName, ImportType importType)
        {
            var model = new ImportMappingViewModel
            {
                BlobName = $"{id}/{fileName}",
                ImportType = importType,
            };

            await using (var fileStream = await _secureFileBlobService.GetStreamFromAzureBlobAsync(model.BlobName))
            using (TextReader textReader = new StreamReader(fileStream))
            using (var reader = new ImportReader(textReader))
            {
                model.CsvFields = reader.GetCsvHeaders();
            }

            return model;
        }

        public IActionResult GetImportFields(ImportType importType)
        {
            var fields1 = StandardFields.GetFields(importType)
                .Where(f => f.Group == null)
                .Select(f => new
                {
                    id = f.Name.ToLowerInvariant(),
                    text = f.Name,
                    allowMulti = f.AllowMulti,
                    isRequired = f.IsRequired,
                    requireOnlyOneOf = f.RequireOnlyOneOf?.Select(s => s.ToLowerInvariant()).ToArray(),
                }).ToArray();

            var fields2 = StandardFields.GetFields(importType)
                .GroupBy(f => f.Group, f => new { f.Name, f.AllowMulti, f.IsRequired })
                .Where(x => x.Key != null)
                .Select(x => new
                {
                    text = x.Key,
                    children = x.Select(f => new { id = f.Name.ToLowerInvariant(), text = f.Name, allowMulti = f.AllowMulti, isRequired = f.IsRequired }),
                }).ToArray();

            return new JsonResult(new { fields1, fields2 });
        }

        public async Task DeleteImportFile(string blobName)
        {
            await _attachmentHelper.DeleteFromAzureBlobAsync(blobName, BlobContainerNames.SecureFiles);
        }


        public async Task<IActionResult> UploadFile()
        {
            var form = await Request.ReadFormAsync();
            var file = form.Files.Any() ? form.Files[0] : null;
            if (file == null)
            {
                return Json(new AjaxResponse(false));
            }

            var id = await _secureFilesService.GetSecureFileDefinitionId();
            string fileName = $"Import_{Clock.Now:yyyyMMddHHmmss}.csv";
            await using (var fileStream = file.OpenReadStream())
            {
                if (fileStream.Length == 0)
                {
                    return Json(new AjaxResponse(false));
                }

                await _secureFileBlobService.UploadSecureFileAsync(fileStream, id, fileName);
            }

            return Json(new AjaxResponse(new { id = id, blobName = fileName }));
        }
    }
}
