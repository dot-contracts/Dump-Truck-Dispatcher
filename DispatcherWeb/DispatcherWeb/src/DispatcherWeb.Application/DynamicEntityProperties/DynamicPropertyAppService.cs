using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.DynamicEntityProperties;
using Abp.UI.Inputs;
using DispatcherWeb.Authorization;
using DispatcherWeb.DynamicEntityProperties.Dto;

namespace DispatcherWeb.DynamicEntityProperties
{
    [AbpAuthorize(AppPermissions.Pages_Administration_DynamicProperties)]
    public class DynamicPropertyAppService : DispatcherWebAppServiceBase, IDynamicPropertyAppService
    {
        private readonly IDynamicPropertyManager _dynamicPropertyManager;
        private readonly IDynamicPropertyStore _dynamicPropertyStore;
        private readonly IDynamicEntityPropertyDefinitionManager _dynamicEntityPropertyDefinitionManager;

        public DynamicPropertyAppService(
            IDynamicPropertyManager dynamicPropertyManager,
            IDynamicPropertyStore dynamicPropertyStore,
            IDynamicEntityPropertyDefinitionManager dynamicEntityPropertyDefinitionManager)
        {
            _dynamicPropertyManager = dynamicPropertyManager;
            _dynamicPropertyStore = dynamicPropertyStore;
            _dynamicEntityPropertyDefinitionManager = dynamicEntityPropertyDefinitionManager;
        }

        public async Task<DynamicPropertyDto> Get(int id)
        {
            var entity = await _dynamicPropertyManager.GetAsync(id);
            return new DynamicPropertyDto
            {
                Id = entity.Id,
                PropertyName = entity.PropertyName,
                DisplayName = entity.DisplayName,
                InputType = entity.InputType,
                Permission = entity.Permission,
                TenantId = entity.TenantId,
            };
        }

        public async Task<ListResultDto<DynamicPropertyDto>> GetAll()
        {
            var entities = await _dynamicPropertyStore.GetAllAsync();

            return new ListResultDto<DynamicPropertyDto>(entities.Select(x => new DynamicPropertyDto
            {
                Id = x.Id,
                PropertyName = x.PropertyName,
                DisplayName = x.DisplayName,
                InputType = x.InputType,
                Permission = x.Permission,
                TenantId = x.TenantId,
            }).ToList());
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Create)]
        public async Task Add(DynamicPropertyDto dto)
        {
            var dynamicProperty = new DynamicProperty
            {
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                PropertyName = dto.PropertyName,
                DisplayName = dto.DisplayName,
                InputType = dto.InputType,
                Permission = dto.Permission,
            };

            await _dynamicPropertyManager.AddAsync(dynamicProperty);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Edit)]
        public async Task Update(DynamicPropertyDto dto)
        {
            var dynamicProperty = new DynamicProperty
            {
                Id = dto.Id,
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                PropertyName = dto.PropertyName,
                DisplayName = dto.DisplayName,
                InputType = dto.InputType,
                Permission = dto.Permission,
            };

            await _dynamicPropertyManager.UpdateAsync(dynamicProperty);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicPropertyValue_Delete)]
        public async Task Delete(int id)
        {
            await _dynamicPropertyManager.DeleteAsync(id);
        }

        public IInputType FindAllowedInputType(string name)
        {
            return _dynamicEntityPropertyDefinitionManager.GetOrNullAllowedInputType(name);
        }
    }
}
