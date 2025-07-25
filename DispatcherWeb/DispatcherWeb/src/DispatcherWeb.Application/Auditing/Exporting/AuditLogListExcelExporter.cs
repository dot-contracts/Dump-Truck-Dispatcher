using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Extensions;
using DispatcherWeb.Auditing.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;

namespace DispatcherWeb.Auditing.Exporting
{
    public class AuditLogListExcelExporter : CsvExporterBase, IAuditLogListExcelExporter
    {
        public AuditLogListExcelExporter(
            ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
        }

        public async Task<FileDto> ExportToFileAsync(List<AuditLogListDto> auditLogListDtos, bool showForAllTenants)
        {
            var timezone = await GetTimezone();
            return await CreateCsvFileAsync(
                "AuditLogs.csv",
                () =>
                {
                    AddHeaderAndData(
                        auditLogListDtos,
                        (L("Time"), x => x.ExecutionTime.ConvertTimeZoneTo(timezone).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")),
                        (showForAllTenants ? L("Tenant") : null, x => x.TenantName),
                        (L("UserName"), x => x.UserName),
                        (L("Service"), x => x.ServiceName),
                        (L("Action"), x => x.MethodName),
                        (L("Parameters"), x => x.Parameters),
                        (L("Duration"), x => x.ExecutionDuration.ToString()),
                        (L("IpAddress"), x => x.ClientIpAddress),
                        (L("Client"), x => x.ClientName),
                        (L("Browser"), x => x.BrowserInfo),
                        (L("ErrorState"), x => x.Exception.IsNullOrEmpty() ? L("Success") : x.Exception)
                    );
                });
        }

        public async Task<FileDto> ExportToFileAsync(List<EntityChangeListDto> entityChangeListDtos)
        {
            var timezone = await GetTimezone();
            return await CreateCsvFileAsync(
                "DetailedLogs.xlsx",
                () =>
                {
                    AddHeaderAndData(
                        entityChangeListDtos,
                        (L("Action"), x => x.ChangeType.ToString()),
                        (L("Object"), x => x.EntityTypeFullName),
                        (L("UserName"), x => x.UserName),
                        (L("Time"), x => x.ChangeTime.ConvertTimeZoneTo(timezone).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"))
                    );
                });
        }
    }
}
