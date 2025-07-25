using System;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using Castle.Core.Logging;
using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class DriverApplicationLogRepository : DispatcherWebRepositoryBase<DriverApplicationLog>, IDriverApplicationLogRepository
    {
        public DriverApplicationLogRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider, ILogger logger)
            : base(dbContextProvider)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }

        public async Task<int> DeleteLogsEarlierThanAsync(DateTime date)
        {
            var context = await GetContextAsync();
            var rowsAffected = await context.Database.ExecuteSqlInterpolatedAsync(
                $"delete from DriverApplicationLog where DateTime < {date}"
            );

            Logger.Info($"DriverApplicationLogRepository.DeleteLogsEarlierThan {date:s}: {rowsAffected} rows affected");

            return rowsAffected;
        }

        public async Task<int> DeleteOldLogsAsync()
        {
            Logger.Info($"DriverApplicationLogRepository.DeleteOldLogs started");

            var context = await GetContextAsync();
            var rowsAffected = await context.Database.ExecuteSqlRawAsync(
                "EXEC RemoveOldDriverAppLogs"
            );

            Logger.Info($"DriverApplicationLogRepository.DeleteOldLogs finished: {rowsAffected} rows affected");

            return rowsAffected;
        }
    }
}
