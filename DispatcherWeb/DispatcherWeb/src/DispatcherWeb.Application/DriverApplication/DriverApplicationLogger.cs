using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Drivers;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.DriverApplication
{
    public class DriverApplicationLogger : IDriverApplicationLogger, ITransientDependency
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<DriverApplicationLog> _driverApplicationLogRepository;
        private readonly Castle.Core.Logging.ILogger _logger;

        public DriverApplicationLogger(
            IRepository<Driver> driverRepository,
            IRepository<DriverApplicationLog> driverApplicationLogRepository,
            ISettingManager settingManager,
            IExtendedAbpSession session,
            Castle.Core.Logging.ILogger logger
            )
        {
            _driverRepository = driverRepository;
            _driverApplicationLogRepository = driverApplicationLogRepository;
            SettingManager = settingManager;
            Session = session;
            _logger = logger;
        }

        public ISettingManager SettingManager { get; }

        public IExtendedAbpSession Session { get; }

        public async Task LogInfo(int driverId, string message)
        {
            await Log(LogLevel.Information, driverId, message);
        }

        public async Task LogInfo(List<int> driverIds, string message)
        {
            await Log(LogLevel.Information, driverIds, message);
        }

        public async Task LogWarn(int driverId, string message)
        {
            await Log(LogLevel.Warning, driverId, message);
        }

        public async Task LogWarn(List<int> driverIds, string message)
        {
            await Log(LogLevel.Warning, driverIds, message);
        }

        public async Task LogError(int driverId, string message)
        {
            await Log(LogLevel.Error, driverId, message);
        }

        public async Task LogError(List<int> driverIds, string message)
        {
            await Log(LogLevel.Error, driverIds, message);
        }

        private async Task Log(LogLevel level, int driverId, string message)
        {
            await Log(level, new List<int> { driverId }, message);
        }

        public async Task Log(LogLevel level, List<int> driverIds, string message)
        {
            var drivers = await (await _driverRepository.GetQueryAsync())
                .Where(x => driverIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.TenantId,
                }).ToListAsync();

            foreach (var driverId in driverIds)
            {
                var driver = drivers.FirstOrDefault(x => x.Id == driverId);

                if (driver == null)
                {
                    _logger.Error($"DriverApplicationLogger.LogInfo: Driver {driverId} wasn't found, skipping this driver");
                    continue;
                }

                if (driver.UserId == null)
                {
                    _logger.Warn($"DriverApplicationLogger.LogInfo: Driver {driverId} doesn't have UserId, skipping this driver");
                    continue;
                }

                var logLevel = await SettingManager.GetSettingValueForUserAsync<int>(AppSettings.DriverApp.LogLevel, driver.TenantId, driver.UserId.Value);
                if (logLevel > (int)level)
                {
                    continue;
                }

                await _driverApplicationLogRepository.InsertAsync(new DriverApplicationLog
                {
                    BatchOrder = 0,
                    DateTime = Clock.Now,
                    DriverId = driverId,
                    Level = level,
                    Message = message,
                    UserId = driver.UserId.Value,
                    TenantId = driver.TenantId,
                });
            }
        }
    }
}
