using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Web.ApplicationInsights
{
    public class ThreadPoolMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TelemetryClient _telemetry;
        private readonly ILogger<ThreadPoolMonitoringMiddleware> _logger;

        public ThreadPoolMonitoringMiddleware(
            RequestDelegate next,
            TelemetryClient telemetry,
            ILogger<ThreadPoolMonitoringMiddleware> logger)
        {
            _next = next;
            _telemetry = telemetry;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var availableCompletionPortThreads);

            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

            var usedWorkerThreads = maxWorkerThreads - availableWorkerThreads;
            var usedCompletionPortThreads = maxCompletionPortThreads - availableCompletionPortThreads;

            _telemetry.TrackMetric("UsedWorkerThreads", usedWorkerThreads);
            _telemetry.TrackMetric("AvailableWorkerThreads", availableWorkerThreads);
            _telemetry.TrackMetric("MaxWorkerThreads", maxWorkerThreads);

            _telemetry.TrackMetric("UsedCompletionPortThreads", usedCompletionPortThreads);
            _telemetry.TrackMetric("AvailableCompletionPortThreads", availableCompletionPortThreads);
            _telemetry.TrackMetric("MaxCompletionPortThreads", maxCompletionPortThreads);

            _telemetry.TrackMetric("PendingWorkItemCount", ThreadPool.PendingWorkItemCount);
            _telemetry.TrackMetric("CompletedWorkItemCount", ThreadPool.CompletedWorkItemCount);
            _telemetry.TrackMetric("ThreadCount", ThreadPool.ThreadCount);

            if (availableWorkerThreads < maxWorkerThreads * 0.2)
            {
                _logger.LogWarning("Thread pool pressure detected: {Available}/{Max} worker threads available",
                    availableWorkerThreads, maxWorkerThreads);
            }

            if (availableCompletionPortThreads < maxCompletionPortThreads * 0.2)
            {
                _logger.LogWarning("I/O completion port pressure detected: {Available}/{Max} completion port threads available",
                    availableCompletionPortThreads, maxCompletionPortThreads);
            }

            await _next(context);
        }
    }
}
