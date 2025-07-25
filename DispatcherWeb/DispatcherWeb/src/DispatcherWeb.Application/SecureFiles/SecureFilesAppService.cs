using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Infrastructure.SecureFiles.Dto;

namespace DispatcherWeb.SecureFiles
{
    public class SecureFilesAppService : DispatcherWebAppServiceBase, ISecureFilesAppService
    {
        private readonly ITenantCache _tenantCache;
        private readonly IRepository<SecureFileDefinition, Guid> _secureFileDefinitionRepository;

        public SecureFilesAppService(
            ITenantCache tenantCache,
            IRepository<SecureFileDefinition, Guid> secureFileDefinitionRepository
        )
        {
            _tenantCache = tenantCache;
            _secureFileDefinitionRepository = secureFileDefinitionRepository;
        }

        private async Task<SecureFileDefinitionDto> PostGetNewLink(SecureFileDefinitionDto dto)
        {
            var entity = await _secureFileDefinitionRepository
                .FirstOrDefaultAsync(x => x.Client == dto.Client && x.Description == dto.Description);
            if (entity == null)
            {
                entity = new SecureFileDefinition
                {
                    Id = new Guid(),
                    Client = dto.Client,
                    Description = dto.Description,
                };
                await _secureFileDefinitionRepository.InsertOrUpdateAsync(entity);
            }
            dto.Id = entity.Id;
            return dto;
        }

        [AbpAuthorize(AppPermissions.Pages_Imports)]
        public async Task<Guid> GetSecureFileDefinitionId()
        {
            var tenant = await _tenantCache.GetAsync(await AbpSession.GetTenantIdAsync());
            var id = (await _secureFileDefinitionRepository.FirstOrDefaultAsync(x => x.Client == tenant.TenancyName))?.Id;
            if (id == null)
            {
                var dto = new SecureFileDefinitionDto
                {
                    Client = tenant.TenancyName,
                    Description = "Created by uploading prospects",
                };
                await PostGetNewLink(dto);
                id = dto.Id;
            }
            return id.Value;
        }
    }
}
