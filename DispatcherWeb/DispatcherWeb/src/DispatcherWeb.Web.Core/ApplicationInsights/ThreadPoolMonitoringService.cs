using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic; // Added missing import for Dictionary

namespace DispatcherWeb.Web.ApplicationInsights
{
    public class ThreadPoolMonitoringService : BackgroundService
    {
        private readonly ILogger<ThreadPoolMonitoringService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly IServiceProvider _serviceProvider;

        public ThreadPoolMonitoringService(
            ILogger<ThreadPoolMonitoringService> logger,
            TelemetryClient telemetryClient,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Thread pool monitoring service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MonitorThreadPoolHealthAsync();
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in thread pool monitoring");
                    _telemetryClient.TrackException(ex);
                }
            }
        }

        private async Task MonitorThreadPoolHealthAsync()
        {
            // Get thread pool statistics
            var availableThreads = ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            var maxThreads = ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

            // Calculate utilization percentages
            var workerThreadUtilization = (double)(maxWorkerThreads - workerThreads) / maxWorkerThreads * 100;
            var completionPortUtilization = (double)(maxCompletionPortThreads - completionPortThreads) / maxCompletionPortThreads * 100;

            // Track metrics
            _telemetryClient.TrackMetric("ThreadPool_WorkerThreadUtilization", workerThreadUtilization);
            _telemetryClient.TrackMetric("ThreadPool_CompletionPortUtilization", completionPortUtilization);
            _telemetryClient.TrackMetric("ThreadPool_AvailableWorkerThreads", workerThreads);
            _telemetryClient.TrackMetric("ThreadPool_AvailableCompletionPortThreads", completionPortThreads);
            _telemetryClient.TrackMetric("ThreadPool_MaxWorkerThreads", maxWorkerThreads);
            _telemetryClient.TrackMetric("ThreadPool_MaxCompletionPortThreads", maxCompletionPortThreads);

            // Alert on high utilization
            if (workerThreadUtilization > 80 || completionPortUtilization > 80)
            {
                _logger.LogWarning("Thread pool utilization high: Worker={Worker:F1}%, Completion={Completion:F1}%", 
                    workerThreadUtilization, completionPortUtilization);

                _telemetryClient.TrackEvent("ThreadPool_HighUtilization_Alert", new Dictionary<string, string>
                {
                    { "WorkerThreadUtilization", workerThreadUtilization.ToString("F1") },
                    { "CompletionPortUtilization", completionPortUtilization.ToString("F1") },
                    { "AvailableWorkerThreads", workerThreads.ToString() },
                    { "AvailableCompletionPortThreads", completionPortThreads.ToString() }
                });
            }

            // Critical alert on thread pool exhaustion
            if (workerThreadUtilization > 90 || completionPortUtilization > 90)
            {
                _logger.LogError("Thread pool exhaustion detected: Worker={Worker:F1}%, Completion={Completion:F1}%", 
                    workerThreadUtilization, completionPortUtilization);

                _telemetryClient.TrackEvent("ThreadPool_Exhaustion_Alert", new Dictionary<string, string>
                {
                    { "WorkerThreadUtilization", workerThreadUtilization.ToString("F1") },
                    { "CompletionPortUtilization", completionPortUtilization.ToString("F1") },
                    { "AvailableWorkerThreads", workerThreads.ToString() },
                    { "AvailableCompletionPortThreads", completionPortThreads.ToString() }
                });
            }

            // Log detailed information for debugging
            if (workerThreadUtilization > 70 || completionPortUtilization > 70)
            {
                _logger.LogInformation("Thread pool status: Worker={Worker:F1}% ({Available}/{Max}), Completion={Completion:F1}% ({AvailableCompletion}/{MaxCompletion})", 
                    workerThreadUtilization, workerThreads, maxWorkerThreads,
                    completionPortUtilization, completionPortThreads, maxCompletionPortThreads);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Thread pool monitoring service stopping");
            await base.StopAsync(cancellationToken);
        }
    }
} 