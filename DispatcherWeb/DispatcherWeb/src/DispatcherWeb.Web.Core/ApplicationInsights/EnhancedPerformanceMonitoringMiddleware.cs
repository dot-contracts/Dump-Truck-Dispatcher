using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Web.ApplicationInsights
{
    public class EnhancedPerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EnhancedPerformanceMonitoringMiddleware> _logger;
        private readonly TelemetryClient _telemetryClient;

        public EnhancedPerformanceMonitoringMiddleware(RequestDelegate next, ILogger<EnhancedPerformanceMonitoringMiddleware> logger, TelemetryClient telemetryClient)
        {
            _next = next;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var startTime = DateTime.UtcNow;

            try
            {
                // Capture thread pool state at start
                var startThreadPoolState = GetThreadPoolState();

                await _next(context);

                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;

                // Capture thread pool state at end
                var endThreadPoolState = GetThreadPoolState();

                // Track comprehensive metrics
                TrackRequestMetrics(context, duration, threadId, startThreadPoolState, endThreadPoolState);

                // Alert on slow requests
                if (duration > 1000)
                {
                    _logger.LogWarning("Slow request detected: {Path} took {Duration}ms on thread {ThreadId}", 
                        context.Request.Path, duration, threadId);
                }

                // Alert on thread pool issues
                if (startThreadPoolState.WorkerThreadUtilization > 80 || endThreadPoolState.WorkerThreadUtilization > 80)
                {
                    _logger.LogWarning("High thread pool utilization during request: {Path}, Start={Start}%, End={End}%", 
                        context.Request.Path, startThreadPoolState.WorkerThreadUtilization, endThreadPoolState.WorkerThreadUtilization);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;
                var threadPoolState = GetThreadPoolState();

                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackMetric("Request_Error_Duration", duration);
                _telemetryClient.TrackEvent("Request_Error", new Dictionary<string, string>
                {
                    { "Path", context.Request.Path },
                    { "Method", context.Request.Method },
                    { "Duration", duration.ToString() },
                    { "ThreadId", threadId.ToString() },
                    { "WorkerThreadUtilization", threadPoolState.WorkerThreadUtilization.ToString("F1") },
                    { "CompletionPortUtilization", threadPoolState.CompletionPortUtilization.ToString("F1") },
                    { "ExceptionType", ex.GetType().Name }
                });

                _logger.LogError(ex, "Request failed: {Path} after {Duration}ms on thread {ThreadId}", 
                    context.Request.Path, duration, threadId);
                throw;
            }
        }

        private void TrackRequestMetrics(HttpContext context, long duration, int threadId, ThreadPoolState startState, ThreadPoolState endState)
        {
            var properties = new Dictionary<string, string>
            {
                { "Path", context.Request.Path },
                { "Method", context.Request.Method },
                { "StatusCode", context.Response.StatusCode.ToString() },
                { "Duration", duration.ToString() },
                { "ThreadId", threadId.ToString() },
                { "StartWorkerThreadUtilization", startState.WorkerThreadUtilization.ToString("F1") },
                { "EndWorkerThreadUtilization", endState.WorkerThreadUtilization.ToString("F1") },
                { "StartCompletionPortUtilization", startState.CompletionPortUtilization.ToString("F1") },
                { "EndCompletionPortUtilization", endState.CompletionPortUtilization.ToString("F1") },
                { "AvailableWorkerThreads", endState.AvailableWorkerThreads.ToString() },
                { "AvailableCompletionPortThreads", endState.AvailableCompletionPortThreads.ToString() }
            };

            // Track request duration
            _telemetryClient.TrackMetric("Request_Duration", duration);

            // Track thread pool utilization during request
            _telemetryClient.TrackMetric("Request_ThreadPool_WorkerUtilization_Start", startState.WorkerThreadUtilization);
            _telemetryClient.TrackMetric("Request_ThreadPool_WorkerUtilization_End", endState.WorkerThreadUtilization);
            _telemetryClient.TrackMetric("Request_ThreadPool_CompletionUtilization_Start", startState.CompletionPortUtilization);
            _telemetryClient.TrackMetric("Request_ThreadPool_CompletionUtilization_End", endState.CompletionPortUtilization);

            // Track slow requests
            if (duration > 1000)
            {
                _telemetryClient.TrackMetric("SlowRequest_Duration", duration);
                _telemetryClient.TrackEvent("SlowRequest", properties);
            }

            // Track all requests for analysis
            _telemetryClient.TrackEvent("Request_Completed", properties);
        }

        private ThreadPoolState GetThreadPoolState()
        {
            var availableThreads = ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            var maxThreads = ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

            var workerThreadUtilization = (double)(maxWorkerThreads - workerThreads) / maxWorkerThreads * 100;
            var completionPortUtilization = (double)(maxCompletionPortThreads - completionPortThreads) / maxCompletionPortThreads * 100;

            return new ThreadPoolState
            {
                AvailableWorkerThreads = workerThreads,
                AvailableCompletionPortThreads = completionPortThreads,
                MaxWorkerThreads = maxWorkerThreads,
                MaxCompletionPortThreads = maxCompletionPortThreads,
                WorkerThreadUtilization = workerThreadUtilization,
                CompletionPortUtilization = completionPortUtilization
            };
        }

        private class ThreadPoolState
        {
            public int AvailableWorkerThreads { get; set; }
            public int AvailableCompletionPortThreads { get; set; }
            public int MaxWorkerThreads { get; set; }
            public int MaxCompletionPortThreads { get; set; }
            public double WorkerThreadUtilization { get; set; }
            public double CompletionPortUtilization { get; set; }
        }
    }
} 