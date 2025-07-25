using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.DynamicEntityProperties;
using DispatcherWeb.Authorization;
using DispatcherWeb.DynamicEntityProperties.Dto;

namespace DispatcherWeb.DynamicEntityProperties
{
    [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue)]
    public class DynamicPropertyValueAppService : DispatcherWebAppServiceBase, IDynamicPropertyValueAppService
    {
        private readonly IDynamicPropertyValueManager _dynamicPropertyValueManager;
        private readonly IDynamicPropertyValueStore _dynamicPropertyValueStore;

        public DynamicPropertyValueAppService(
            IDynamicPropertyValueManager dynamicPropertyValueManager,
            IDynamicPropertyValueStore dynamicPropertyValueStore
        )
        {
            _dynamicPropertyValueManager = dynamicPropertyValueManager;
            _dynamicPropertyValueStore = dynamicPropertyValueStore;
        }

        public async Task<DynamicPropertyValueDto> Get(int id)
        {
            var entity = await _dynamicPropertyValueManager.GetAsync(id);
            return new DynamicPropertyValueDto
            {
                Id = entity.Id,
                Value = entity.Value,
                DynamicPropertyId = entity.DynamicPropertyId,
                TenantId = entity.TenantId,
            };
        }

        public async Task<ListResultDto<DynamicPropertyValueDto>> GetAllValuesOfDynamicProperty(EntityDto input)
        {
            var entities = await _dynamicPropertyValueStore.GetAllValuesOfDynamicPropertyAsync(input.Id);
            return new ListResultDto<DynamicPropertyValueDto>(entities.Select(x => new DynamicPropertyValueDto
            {
                Id = x.Id,
                Value = x.Value,
                TenantId = x.TenantId,
                DynamicPropertyId = x.DynamicPropertyId,
            }).ToList());
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Create)]
        public async Task Add(DynamicPropertyValueDto dto)
        {
            var dynamicPropertyValue = new DynamicPropertyValue
            {
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                Value = dto.Value,
                DynamicPropertyId = dto.DynamicPropertyId,
            };

            await _dynamicPropertyValueManager.AddAsync(dynamicPropertyValue);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Edit)]
        public async Task Update(DynamicPropertyValueDto dto)
        {
            var dynamicPropertyValue = new DynamicPropertyValue
            {
                Id = dto.Id,
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                Value = dto.Value,
                DynamicPropertyId = dto.DynamicPropertyId,
            };

            await _dynamicPropertyValueManager.UpdateAsync(dynamicPropertyValue);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Delete)]
        public async Task Delete(int id)
        {
            await _dynamicPropertyValueManager.DeleteAsync(id);
        }
    }
}
