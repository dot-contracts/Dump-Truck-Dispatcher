using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Abp.Domain.Uow;

namespace DispatcherWeb.Web.ApplicationInsights
{
    public class UnitOfWorkMonitoringService : BackgroundService
    {
        private readonly ILogger<UnitOfWorkMonitoringService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly IServiceProvider _serviceProvider;

        public UnitOfWorkMonitoringService(
            ILogger<UnitOfWorkMonitoringService> logger,
            TelemetryClient telemetryClient,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Unit of work monitoring service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MonitorUnitOfWorkHealthAsync();
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in unit of work monitoring");
                    _telemetryClient.TrackException(ex);
                }
            }
        }

        private async Task MonitorUnitOfWorkHealthAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWorkManager = scope.ServiceProvider.GetService<IUnitOfWorkManager>();

                if (unitOfWorkManager != null)
                {
                    var currentUnitOfWork = unitOfWorkManager.Current;
                    
                    if (currentUnitOfWork != null)
                    {
                        _telemetryClient.TrackEvent("UnitOfWork_Active", new Dictionary<string, string>
                        {
                            { "IsActive", currentUnitOfWork.IsActive.ToString() },
                            { "HasChanges", currentUnitOfWork.HasChanges.ToString() },
                            { "Options", currentUnitOfWork.Options?.ToString() ?? "null" }
                        });

                        _telemetryClient.TrackMetric("UnitOfWork_ActiveCount", 1);
                    }
                    else
                    {
                        _telemetryClient.TrackMetric("UnitOfWork_ActiveCount", 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error monitoring unit of work");
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackEvent("UnitOfWork_Monitoring_Error", new Dictionary<string, string>
                {
                    { "ExceptionType", ex.GetType().Name },
                    { "ExceptionMessage", ex.Message }
                });
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Unit of work monitoring service stopping");
            await base.StopAsync(cancellationToken);
        }
    }
} 