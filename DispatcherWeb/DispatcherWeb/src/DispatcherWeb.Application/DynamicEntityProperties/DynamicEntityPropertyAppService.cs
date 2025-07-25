using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.DynamicEntityProperties;
using DispatcherWeb.Authorization;
using DispatcherWeb.DynamicEntityProperties.Dto;

namespace DispatcherWeb.DynamicEntityProperties
{
    [AbpAuthorize(AppPermissions.Pages_Administration_DynamicEntityProperties)]
    public class DynamicEntityPropertyAppService : DispatcherWebAppServiceBase, IDynamicEntityPropertyAppService
    {
        private readonly IDynamicEntityPropertyManager _dynamicEntityPropertyManager;

        public DynamicEntityPropertyAppService(IDynamicEntityPropertyManager dynamicEntityPropertyManager)
        {
            _dynamicEntityPropertyManager = dynamicEntityPropertyManager;
        }

        public async Task<DynamicEntityPropertyDto> Get(int id)
        {
            var dynamicEntityProperty = await _dynamicEntityPropertyManager.GetAsync(id);

            return new DynamicEntityPropertyDto
            {
                EntityFullName = dynamicEntityProperty.EntityFullName,
                DynamicPropertyName = dynamicEntityProperty.DynamicProperty.PropertyName,
                DynamicPropertyId = dynamicEntityProperty.DynamicProperty.Id,
            };
        }

        public async Task<ListResultDto<DynamicEntityPropertyDto>> GetAllPropertiesOfAnEntity(DynamicEntityPropertyGetAllInput input)
        {
            var entities = (await _dynamicEntityPropertyManager.GetAllAsync(input.EntityFullName))
                .Select(x => new DynamicEntityPropertyDto
                {
                    Id = x.Id,
                    EntityFullName = x.EntityFullName,
                    DynamicPropertyName = x.DynamicProperty.PropertyName,
                    DynamicPropertyId = x.DynamicPropertyId,
                }).ToList();
            return new ListResultDto<DynamicEntityPropertyDto>(entities);

        }

        public async Task<ListResultDto<DynamicEntityPropertyDto>> GetAll()
        {
            var entities = (await _dynamicEntityPropertyManager.GetAllAsync())
                .Select(x => new DynamicEntityPropertyDto
                {
                    Id = x.Id,
                    DynamicPropertyId = x.DynamicPropertyId,
                    DynamicPropertyName = x.DynamicProperty.DisplayName,
                    EntityFullName = x.EntityFullName,
                }).ToList();
            return new ListResultDto<DynamicEntityPropertyDto>(entities);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicEntityProperties_Create)]
        public async Task Add(DynamicEntityPropertyDto dto)
        {
            var tenantId = await AbpSession.GetTenantIdOrNullAsync();
            dto.TenantId = tenantId;
            await _dynamicEntityPropertyManager.AddAsync(new DynamicEntityProperty
            {
                EntityFullName = dto.EntityFullName,
                DynamicPropertyId = dto.DynamicPropertyId,
                TenantId = tenantId,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicEntityProperties_Edit)]
        public async Task Update(DynamicEntityPropertyDto dto)
        {
            await _dynamicEntityPropertyManager.UpdateAsync(new DynamicEntityProperty
            {
                Id = dto.Id,
                EntityFullName = dto.EntityFullName,
                DynamicPropertyId = dto.DynamicPropertyId,
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_DynamicEntityProperties_Delete)]
        public async Task Delete(int id)
        {
            await _dynamicEntityPropertyManager.DeleteAsync(id);
        }

        public async Task<ListResultDto<GetAllEntitiesHasDynamicPropertyOutput>> GetAllEntitiesHasDynamicProperty()
        {
            var entities = await _dynamicEntityPropertyManager.GetAllAsync();
            return new ListResultDto<GetAllEntitiesHasDynamicPropertyOutput>(
                entities?.Select(x => new GetAllEntitiesHasDynamicPropertyOutput
                {
                    EntityFullName = x.EntityFullName,
                }).DistinctBy(x => x.EntityFullName).ToList()
            );
        }
    }
}
