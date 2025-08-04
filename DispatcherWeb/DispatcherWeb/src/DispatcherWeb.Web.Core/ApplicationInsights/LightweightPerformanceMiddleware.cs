using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Web.ApplicationInsights
{
    public class LightweightPerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LightweightPerformanceMiddleware> _logger;
        private readonly TelemetryClient _telemetryClient;
        private const int SlowRequestThresholdMs = 2000; // Only track requests >2 seconds

        public LightweightPerformanceMiddleware(RequestDelegate next, ILogger<LightweightPerformanceMiddleware> logger, TelemetryClient telemetryClient)
        {
            _next = next;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var threadId = Thread.CurrentThread.ManagedThreadId;

            try
            {
                await _next(context);
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;

                // Only track requests that take longer than 2 seconds (the problematic 1%)
                if (duration > SlowRequestThresholdMs)
                {
                    TrackSlowRequest(context, duration, threadId);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;

                // Track exceptions for slow requests
                if (duration > SlowRequestThresholdMs)
                {
                    TrackSlowRequestWithException(context, duration, threadId, ex);
                }

                throw;
            }
        }

        private void TrackSlowRequest(HttpContext context, long duration, int threadId)
        {
            var threadPoolState = GetThreadPoolState();
            
            _telemetryClient.TrackEvent("SlowRequest_Detected", new Dictionary<string, string>
            {
                { "Path", context.Request.Path },
                { "Method", context.Request.Method },
                { "StatusCode", context.Response.StatusCode.ToString() },
                { "Duration", duration.ToString() },
                { "ThreadId", threadId.ToString() },
                { "WorkerThreadUtilization", threadPoolState.WorkerThreadUtilization.ToString("F1") },
                { "AvailableWorkerThreads", threadPoolState.AvailableWorkerThreads.ToString() },
                { "MaxWorkerThreads", threadPoolState.MaxWorkerThreads.ToString() }
            });

            _telemetryClient.TrackMetric("SlowRequest_Duration", duration);
            _telemetryClient.TrackMetric("SlowRequest_ThreadPool_WorkerUtilization", threadPoolState.WorkerThreadUtilization);

            _logger.LogWarning("Slow request detected: {Path} took {Duration}ms on thread {ThreadId} (ThreadPool: {WorkerUtilization:F1}%)",
                context.Request.Path, duration, threadId, threadPoolState.WorkerThreadUtilization);
        }

        private void TrackSlowRequestWithException(HttpContext context, long duration, int threadId, Exception ex)
        {
            var threadPoolState = GetThreadPoolState();
            
            _telemetryClient.TrackException(ex);
            _telemetryClient.TrackEvent("SlowRequest_Error", new Dictionary<string, string>
            {
                { "Path", context.Request.Path },
                { "Method", context.Request.Method },
                { "Duration", duration.ToString() },
                { "ThreadId", threadId.ToString() },
                { "WorkerThreadUtilization", threadPoolState.WorkerThreadUtilization.ToString("F1") },
                { "ExceptionType", ex.GetType().Name },
                { "ExceptionMessage", ex.Message }
            });

            _telemetryClient.TrackMetric("SlowRequest_Error_Duration", duration);

            _logger.LogError(ex, "Slow request with error: {Path} took {Duration}ms on thread {ThreadId}",
                context.Request.Path, duration, threadId);
        }

        private ThreadPoolState GetThreadPoolState()
        {
            var availableThreads = ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            var maxThreads = ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

            var workerThreadUtilization = (double)(maxWorkerThreads - workerThreads) / maxWorkerThreads * 100;

            return new ThreadPoolState
            {
                AvailableWorkerThreads = workerThreads,
                MaxWorkerThreads = maxWorkerThreads,
                WorkerThreadUtilization = workerThreadUtilization
            };
        }

        private class ThreadPoolState
        {
            public int AvailableWorkerThreads { get; set; }
            public int MaxWorkerThreads { get; set; }
            public double WorkerThreadUtilization { get; set; }
        }
    }
} 