using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System.Collections.Generic; // Added missing import for Dictionary

namespace DispatcherWeb.Web.ApplicationInsights
{
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly TelemetryClient _telemetryClient;

        public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger, TelemetryClient telemetryClient)
        {
            _next = next;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                using var memoryStream = new System.IO.MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context);

                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);

                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;

                // Track slow requests (> 1000ms)
                if (duration > 1000)
                {
                    _telemetryClient.TrackMetric("SlowRequest_Duration", duration);
                    _telemetryClient.TrackEvent("SlowRequest", new Dictionary<string, string>
                    {
                        { "Path", context.Request.Path },
                        { "Method", context.Request.Method },
                        { "StatusCode", context.Response.StatusCode.ToString() },
                        { "Duration", duration.ToString() }
                    });
                    _logger.LogWarning("Slow request detected: {Path} took {Duration}ms", context.Request.Path, duration);
                }

                // Track all requests for performance analysis
                _telemetryClient.TrackMetric("Request_Duration", duration);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;
                
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackMetric("Request_Error_Duration", duration);
                _telemetryClient.TrackEvent("Request_Error", new Dictionary<string, string>
                {
                    { "Path", context.Request.Path },
                    { "Method", context.Request.Method },
                    { "Duration", duration.ToString() }
                });
                
                _logger.LogError(ex, "Request failed: {Path} after {Duration}ms", context.Request.Path, duration);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
} 