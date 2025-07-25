using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.VehicleMaintenance;
using DispatcherWeb.VehicleServices.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.VehicleServices
{
    [AbpAuthorize]
    public class VehicleServiceAppService : DispatcherWebAppServiceBase, IVehicleServiceAppService
    {
        private readonly IAttachmentHelper _attachmentHelper;
        private readonly IRepository<VehicleService> _vehicleServiceRepository;
        private readonly IRepository<VehicleServiceDocument> _vehicleServiceDocumentRepository;

        public VehicleServiceAppService(
            IAttachmentHelper attachmentHelper,
            IRepository<VehicleService> vehicleServiceRepository,
            IRepository<VehicleServiceDocument> vehicleServiceDocumentRepository
        )
        {
            _attachmentHelper = attachmentHelper;
            _vehicleServiceRepository = vehicleServiceRepository;
            _vehicleServiceDocumentRepository = vehicleServiceDocumentRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleService_View)]
        public async Task<PagedResultDto<VehicleServiceDto>> GetPagedList(GetVehicleServicesInput input)
        {
            var query = (await _vehicleServiceRepository.GetQueryAsync())
                .WhereIf(!string.IsNullOrEmpty(input.Name), x => x.Name.Contains(input.Name));

            var totalCount = query.Count();

            var items = await query
                .Select(x => new VehicleServiceDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    RecommendedTimeInterval = x.RecommendedTimeInterval,
                    RecommendedHourInterval = x.RecommendedHourInterval,
                    RecommendedMileageInterval = x.RecommendedMileageInterval,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<VehicleServiceDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleService_Edit)]
        public async Task<VehicleServiceEditDto> GetForEdit(NullableIdDto input)
        {
            VehicleServiceEditDto modelDto;
            if (input.Id.HasValue)
            {
                modelDto = await (await _vehicleServiceRepository.GetQueryAsync())
                    .Select(x => new VehicleServiceEditDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        RecommendedTimeInterval = x.RecommendedTimeInterval,
                        RecommendedHourInterval = (int)x.RecommendedHourInterval,
                        RecommendedMileageInterval = x.RecommendedMileageInterval,
                        WarningDays = x.WarningDays,
                        WarningHours = (int)x.WarningHours,
                        WarningMiles = x.WarningMiles,
                        Documents = x.Documents.Select(d => new VehicleServiceDocumentEditDto
                        {
                            Id = d.Id,
                            VehicleServiceId = d.VehicleServiceId,
                            FileId = d.FileId,
                            Name = d.Name,
                            Description = d.Description,
                        }).ToList(),
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                modelDto = new VehicleServiceEditDto();
            }
            return modelDto;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleService_Edit)]
        public async Task<VehicleServiceEditDto> Save(VehicleServiceEditDto model)
        {
            var entity = model.Id != 0
                ? await GetVehicleServiceWithDocuments(model.Id)
                : new VehicleService();
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.RecommendedTimeInterval = model.RecommendedTimeInterval;
            entity.RecommendedHourInterval = model.RecommendedHourInterval;
            entity.RecommendedMileageInterval = model.RecommendedMileageInterval;
            entity.WarningDays = model.WarningDays;
            entity.WarningHours = model.WarningHours;
            entity.WarningMiles = model.WarningMiles;

            if (await (await _vehicleServiceRepository.GetQueryAsync())
                .WhereIf(model.Id != 0, x => x.Id != model.Id)
                .AnyAsync(x => x.Name == model.Name)
            )
            {
                throw new UserFriendlyException($"Service with name '{model.Name}' already exists!");
            }


            foreach (var documentEntity in entity.Documents.ToList())
            {
                var modelDocument = model.Documents.FirstOrDefault(x => x.Id == documentEntity.Id);
                if (modelDocument == null)
                {
                    await DeleteDocument(documentEntity);
                    continue;
                }
                modelDocument.VehicleServiceId = model.Id;
                CopyDocumentProperties(documentEntity, modelDocument);
            }

            model.Id = await _vehicleServiceRepository.InsertOrUpdateAndGetIdAsync(entity);
            return model;
        }

        private async Task<VehicleService> GetVehicleServiceWithDocuments(int id)
        {
            return await (await _vehicleServiceRepository.GetQueryAsync())
                .Include(x => x.Documents)
                .SingleAsync(x => x.Id == id);
        }

        private async Task DeleteDocument(VehicleServiceDocument documentEntity)
        {
            await _attachmentHelper.DeleteFromAzureBlobAsync($"{documentEntity.VehicleServiceId}/{documentEntity.FileId}", BlobContainerNames.VehicleServiceDocuments);
            await _vehicleServiceDocumentRepository.DeleteAsync(documentEntity);
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleService_Edit)]
        public async Task<bool> Delete(int id)
        {
            await CheckVehicleServiceDependencies(id);
            var vehicleEntity = await GetVehicleServiceWithDocuments(id);
            foreach (var documentEntity in vehicleEntity.Documents.ToList())
            {
                await DeleteDocument(documentEntity);
            }
            await _vehicleServiceRepository.DeleteAsync(vehicleEntity);
            return true;
        }

        private async Task CheckVehicleServiceDependencies(int id)
        {
            if (await (await _vehicleServiceRepository.GetQueryAsync()).Where(x => x.Id == id).AnyAsync(x => x.PreventiveMaintenance.Any()))
            {
                throw new UserFriendlyException("Cannot delete the Service because there are Preventive Maintenance associated with this Service.");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleService_Edit)]
        public async Task<VehicleServiceDocumentEditDto> SaveDocument(VehicleServiceDocumentEditDto model)
        {
            var entity = model.Id != 0
                ? await _vehicleServiceDocumentRepository.GetAsync(model.Id)
                : new VehicleServiceDocument();
            CopyDocumentProperties(entity, model);

            model.Id = await _vehicleServiceDocumentRepository.InsertOrUpdateAndGetIdAsync(entity);
            return model;
        }

        private void CopyDocumentProperties(VehicleServiceDocument entity, VehicleServiceDocumentEditDto model)
        {
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.VehicleServiceId = model.VehicleServiceId;
            entity.FileId = model.FileId;
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_VehicleServices)]
        public async Task<PagedResultDto<SelectListDto>> GetSelectList(GetSelectListInput input)
        {
            var query = (await _vehicleServiceRepository.GetQueryAsync())
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                });

            return await query.GetSelectListResult(input);
        }
    }
}
