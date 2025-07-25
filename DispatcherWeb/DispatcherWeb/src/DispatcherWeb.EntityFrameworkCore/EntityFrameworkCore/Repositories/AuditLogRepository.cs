using System.Threading.Tasks;
using Abp.Auditing;
using Abp.EntityFrameworkCore;
using Castle.Core.Logging;
using DispatcherWeb.Auditing;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class AuditLogRepository : DispatcherWebRepositoryBase<AuditLog, long>, IAuditLogRepository
    {
        public AuditLogRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider, ILogger logger)
            : base(dbContextProvider)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }

        public async Task<int> DeleteOldAuditLogsAsync()
        {
            Logger.Info($"AuditLogRepository.DeleteOldAuditLogs started");

            var context = await GetContextAsync();
            var rowsAffected = await context.Database.ExecuteSqlRawAsync(
                "EXEC RemoveOldAuditLogs"
            );

            Logger.Info($"AuditLogRepository.DeleteOldAuditLogs finished: {rowsAffected} rows affected");

            return rowsAffected;
        }
    }
}
