using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.VehicleMaintenance;
using DispatcherWeb.VehicleServiceTypes.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.VehicleServiceTypes
{
    [AbpAuthorize]
    public class VehicleServiceTypeAppService : DispatcherWebAppServiceBase, IVehicleServiceTypeAppService
    {
        private readonly IRepository<VehicleServiceType> _vehicleServiceTypeRepository;

        public VehicleServiceTypeAppService(
            IRepository<VehicleServiceType> vehicleServiceTypeRepository
        )
        {
            _vehicleServiceTypeRepository = vehicleServiceTypeRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleServiceTypes_View)]
        public async Task<IList<VehicleServiceTypeDto>> GetList()
        {
            return await (await _vehicleServiceTypeRepository.GetQueryAsync())
                .Select(x => new VehicleServiceTypeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .ToListAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleServiceTypes_Edit)]
        public async Task<VehicleServiceTypeDto> Save(VehicleServiceTypeDto model)
        {
            if (await (await _vehicleServiceTypeRepository.GetQueryAsync())
                .WhereIf(model.Id != 0, x => x.Id != model.Id)
                .AnyAsync(x => x.Name == model.Name)
            )
            {
                throw new UserFriendlyException($"Service type with name '{model.Name}' already exists!");
            }

            VehicleServiceType entity;
            if (model.Id != 0)
            {
                entity = await _vehicleServiceTypeRepository.GetAsync(model.Id);
            }
            else
            {
                entity = new VehicleServiceType();
            }
            entity.Name = model.Name;
            model.Id = await _vehicleServiceTypeRepository.InsertOrUpdateAndGetIdAsync(entity);
            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_VehicleServiceTypes_Edit)]
        public async Task<bool> Delete(int id)
        {
            if (await ServiceTypeInUse(id))
            {
                throw new UserFriendlyException("This type is being used by Work Orders and can’t be deleted.");
            }
            await _vehicleServiceTypeRepository.DeleteAsync(id);
            return true;
        }

        private async Task<bool> ServiceTypeInUse(int id)
        {
            return await (await _vehicleServiceTypeRepository.GetQueryAsync())
                .AnyAsync(x => x.Id == id && x.WorkOrders.Any())
                ;
        }

        [AbpAuthorize(
            AppPermissions.Pages_VehicleServiceTypes_View,
            AppPermissions.Pages_VehicleService_View,
            AppPermissions.Pages_PreventiveMaintenanceSchedule_View,
            AppPermissions.Pages_WorkOrders_View
        )]
        public async Task<PagedResultDto<SelectListDto>> GetSelectList(GetSelectListInput input)
        {
            var items = await (await _vehicleServiceTypeRepository.GetQueryAsync())
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                })
                .GetSelectListResult(input);

            return items;
        }


    }
}
