using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Uow;
using DispatcherWeb.Auditing.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Auditing
{
    public interface IAuditLogAppService : IApplicationService
    {
        Task<PagedResultDto<AuditLogListDto>> GetAuditLogs(GetAuditLogsInput input);

        Task<FileDto> GetAuditLogsToExcel(GetAuditLogsInput input);

        Task<PagedResultDto<EntityChangeListDto>> GetEntityChanges(GetEntityChangeInput input);

        Task<PagedResultDto<EntityChangeListDto>> GetEntityTypeChanges(GetEntityChangeInput input);

        Task<FileDto> GetEntityChangesToExcel(GetEntityChangeInput input);

        Task<List<EntityPropertyChangeDto>> GetEntityPropertyChanges(long entityChangeId);

        Task<List<NameValueDto>> GetEntityHistoryObjectTypesAsync();

        [AbpAllowAnonymous, RemoteService(false), UnitOfWork(false)]
        Task RemoveOldAuditLogsAsync();
    }
}
