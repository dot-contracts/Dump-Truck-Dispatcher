using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Auditing.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Auditing.Exporting
{
    public interface IAuditLogListExcelExporter
    {
        Task<FileDto> ExportToFileAsync(List<AuditLogListDto> auditLogListDtos, bool showForAllTenants);

        Task<FileDto> ExportToFileAsync(List<EntityChangeListDto> entityChangeListDtos);
    }
}
