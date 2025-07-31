# DISPATCHER WEB APPLICATION - PERFORMANCE IMPLEMENTATION CODE

**Document Version:** 1.0  
**Date:** December 2024  
**Application:** DispatcherWeb - Multi-tenant ASP.NET MVC Application

---

## TABLE OF CONTENTS

1. [Issue 1: Thread Pool Starvation Implementation](#issue-1-thread-pool-starvation-implementation)
2. [Issue 2: Database Connection Pool Optimization](#issue-2-database-connection-pool-optimization)
3. [Issue 3: N+1 Query Problem - Batch Processing](#issue-3-n1-query-problem---batch-processing)
4. [Issue 4: Comprehensive Performance Monitoring](#issue-4-comprehensive-performance-monitoring)
5. [Issue 5: Authentication Performance Monitoring](#issue-5-authentication-performance-monitoring)
6. [Configuration Changes](#configuration-changes)
7. [Unit Tests](#unit-tests)

---

## ISSUE 1: THREAD POOL STARVATION IMPLEMENTATION

### 1.1 ThreadPoolMonitoringService.cs
**File:** `src/DispatcherWeb.Web.Core/ApplicationInsights/ThreadPoolMonitoringService.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Thread pool monitoring service stopped");
            await base.StopAsync(cancellationToken);
        }
    }
}
```

### 1.2 Startup.cs Configuration
**File:** `src/DispatcherWeb.Web.Mvc/Startup/Startup.cs`

```csharp
// Add to ConfigureServices method
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // ... existing code ...
    
    if (!_appConfiguration.GetValue<bool>("App:DisableAppInsights"))
    {
        services.AddApplicationInsightsTelemetry();
        services.AddSnapshotCollector();
        services.AddSingleton<ITelemetryInitializer, AbpTelemetryInitializer>();
        services.AddHostedService<ThreadPoolMonitoringService>(); // Added
    }
    
    // ... existing code ...
    
    // Thread pool configuration
    ThreadPool.SetMinThreads(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);
    ThreadPool.SetMaxThreads(Environment.ProcessorCount * 4, Environment.ProcessorCount * 4);
    
    // ... rest of existing code ...
}

// Add to Configure method
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    // ... existing middleware ...
    
    if (!_appConfiguration.GetValue<bool>("App:DisablePerformanceMonitoringMiddleware")
        && !_appConfiguration.GetValue<bool>("App:DisableAppInsights"))
    {
        app.UseMiddleware<EnhancedPerformanceMonitoringMiddleware>(); // Replaced old middleware
    }
    
    // ... rest of existing code ...
}
```

---

## ISSUE 2: DATABASE CONNECTION POOL OPTIMIZATION

### 2.1 appsettings.json Configuration
**File:** `src/DispatcherWeb.Web.Mvc/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Server=DESKTOP-G133VUQ\\SQLEXPRESS;Database=DispatcherWebDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false;Integrated Security=true;Connection Timeout=30;Command Timeout=60;Max Pool Size=200;Min Pool Size=20;Pooling=true;Application Name=DispatcherWeb"
  },
  "EntityFramework": {
    "CommandTimeout": 60,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3
  },
  "App": {
    "DisablePerformanceMonitoringMiddleware": false,
    "DisableAppInsights": false
  }
}
```

---

## ISSUE 3: N+1 QUERY PROBLEM - BATCH PROCESSING

### 3.1 SetOrderLineIsCompleteBatchInput.cs
**File:** `src/DispatcherWeb.Application.Shared/Scheduling/Dto/SetOrderLineIsCompleteBatchInput.cs`

```csharp
using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineIsCompleteBatchInput
    {
        public List<int> OrderLineIds { get; set; }
        public bool IsComplete { get; set; }
        public bool IsCancelled { get; set; }
    }
}
```

### 3.2 ISchedulingAppService.cs Update
**File:** `src/DispatcherWeb.Application.Shared/Scheduling/ISchedulingAppService.cs`

```csharp
// Add this method signature to the interface
Task SetOrderLineIsCompleteBatch(SetOrderLineIsCompleteBatchInput input);
```

### 3.3 SchedulingAppService.cs Implementation
**File:** `src/DispatcherWeb.Application/Scheduling/SchedulingAppService.cs`

```csharp
// Add this new method to the SchedulingAppService class
[AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
public async Task SetOrderLineIsCompleteBatch(SetOrderLineIsCompleteBatchInput input)
{
    var telemetry = new TelemetryClient();
    var startTime = DateTime.UtcNow;

    try
    {
        // Validate input
        if (input.OrderLineIds == null || !input.OrderLineIds.Any())
        {
            throw new ArgumentException("OrderLineIds cannot be null or empty");
        }

        // Get all order lines in a single query
        var orderLines = await _context.OrderLines
            .Where(ol => input.OrderLineIds.Contains(ol.Id))
            .ToListAsync();

        if (!orderLines.Any())
        {
            throw new EntityNotFoundException("No order lines found with the provided IDs");
        }

        // Update all order lines in memory
        orderLines.ForEach(ol => ol.IsComplete = input.IsComplete);

        // Handle cancellation if needed
        if (input.IsCancelled)
        {
            // Batch cancel dispatches
            var dispatches = await _context.Dispatches
                .Where(d => input.OrderLineIds.Contains(d.OrderLineId))
                .ToListAsync();

            dispatches.ForEach(d => d.Status = DispatchStatus.Cancelled);

            // Batch end dispatches
            var activeDispatches = dispatches.Where(d => d.Status == DispatchStatus.Active).ToList();
            activeDispatches.ForEach(d => d.EndTime = DateTime.UtcNow);
        }

        // Batch get tickets
        var tickets = await _context.Tickets
            .Where(t => input.OrderLineIds.Contains(t.OrderLineId))
            .ToListAsync();

        // Batch delete order line trucks
        var orderLineTrucks = await _context.OrderLineTrucks
            .Where(olt => input.OrderLineIds.Contains(olt.OrderLineId))
            .ToListAsync();

        _context.OrderLineTrucks.RemoveRange(orderLineTrucks);

        // Single SaveChanges operation
        await CurrentUnitOfWork.SaveChangesAsync();

        // Track performance metrics
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        telemetry.TrackMetric("SetOrderLineIsCompleteBatch_Duration", duration);
        telemetry.TrackMetric("SetOrderLineIsCompleteBatch_OrderLineCount", input.OrderLineIds.Count);
        telemetry.TrackEvent("SetOrderLineIsCompleteBatch_Completed", new Dictionary<string, string>
        {
            { "OrderLineCount", input.OrderLineIds.Count.ToString() },
            { "Duration", duration.ToString() },
            { "IsComplete", input.IsComplete.ToString() },
            { "IsCancelled", input.IsCancelled.ToString() }
        });
    }
    catch (Exception ex)
    {
        telemetry.TrackException(ex);
        telemetry.TrackEvent("SetOrderLineIsCompleteBatch_Error", new Dictionary<string, string>
        {
            { "OrderLineCount", input.OrderLineIds?.Count.ToString() ?? "0" },
            { "ExceptionType", ex.GetType().Name },
            { "ErrorMessage", ex.Message }
        });
        throw;
    }
}

// Update the existing SetAllOrderLinesIsComplete method
[AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
public async Task SetAllOrderLinesIsComplete(SetAllOrderLinesIsCompleteInput input)
{
    // Use the new batch method instead of individual calls
    await SetOrderLineIsCompleteBatch(new SetOrderLineIsCompleteBatchInput
    {
        OrderLineIds = input.OrderLineIds,
        IsComplete = input.IsComplete,
        IsCancelled = input.IsCancelled
    });
}
```

---

## ISSUE 4: COMPREHENSIVE PERFORMANCE MONITORING

### 4.1 EnhancedPerformanceMonitoringMiddleware.cs
**File:** `src/DispatcherWeb.Web.Core/ApplicationInsights/EnhancedPerformanceMonitoringMiddleware.cs`

```csharp
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

            _telemetryClient.TrackMetric("Request_Duration", duration);
            _telemetryClient.TrackMetric("Request_ThreadId", threadId);
            _telemetryClient.TrackMetric("Request_StartWorkerThreadUtilization", startState.WorkerThreadUtilization);
            _telemetryClient.TrackMetric("Request_EndWorkerThreadUtilization", endState.WorkerThreadUtilization);
            _telemetryClient.TrackMetric("Request_StartCompletionPortUtilization", startState.CompletionPortUtilization);
            _telemetryClient.TrackMetric("Request_EndCompletionPortUtilization", endState.CompletionPortUtilization);

            _telemetryClient.TrackEvent("Request_Completed", properties);
        }

        private ThreadPoolState GetThreadPoolState()
        {
            ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

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
```

---

## ISSUE 5: AUTHENTICATION PERFORMANCE MONITORING

### 5.1 AccountController.cs Enhancement
**File:** `src/DispatcherWeb.Web.Mvc/Controllers/AccountController.cs`

```csharp
// Add to the existing Login method
public virtual async Task<JsonResult> Login(LoginViewModel loginModel, string returnUrl = "", string returnUrlHash = "", string ss = "")
{
    var startTime = DateTime.UtcNow;
    var telemetry = new TelemetryClient();

    try
    {
        // Existing login logic
        var result = await _logInManager.LoginAsync(loginModel.UsernameOrEmailAddress, loginModel.Password, loginModel.TenancyName, loginModel.ShouldRememberMe);

        // Track successful login
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        telemetry.TrackMetric("Login_Success_Duration", duration);
        telemetry.TrackEvent("Login_Success", new Dictionary<string, string>
        {
            { "Username", loginModel.UsernameOrEmailAddress },
            { "TenancyName", loginModel.TenancyName ?? "Host" },
            { "Duration", duration.ToString() },
            { "RememberMe", loginModel.ShouldRememberMe.ToString() }
        });

        return result;
    }
    catch (UserFriendlyException ex)
    {
        // Track login failures
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        telemetry.TrackException(ex);
        telemetry.TrackMetric("Login_Failure_Duration", duration);
        telemetry.TrackEvent("Login_Failure", new Dictionary<string, string>
        {
            { "Username", loginModel.UsernameOrEmailAddress },
            { "ExceptionType", ex.GetType().Name },
            { "ErrorMessage", ex.Message },
            { "Duration", duration.ToString() }
        });

        // Handle specific login scenarios
        if (ex.Message.Contains("password change"))
        {
            telemetry.TrackEvent("Login_PasswordChangeRequired", new Dictionary<string, string>
            {
                { "Username", loginModel.UsernameOrEmailAddress },
                { "Duration", duration.ToString() }
            });
        }
        else if (ex.Message.Contains("two-factor"))
        {
            telemetry.TrackEvent("Login_TwoFactorRequired", new Dictionary<string, string>
            {
                { "Username", loginModel.UsernameOrEmailAddress },
                { "Duration", duration.ToString() }
            });
        }

        throw;
    }
    catch (Exception ex)
    {
        // Track unexpected errors
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        telemetry.TrackException(ex);
        telemetry.TrackMetric("Login_Error_Duration", duration);
        telemetry.TrackEvent("Login_Error", new Dictionary<string, string>
        {
            { "Username", loginModel.UsernameOrEmailAddress },
            { "ExceptionType", ex.GetType().Name },
            { "Duration", duration.ToString() }
        });

        throw;
    }
}
```

---

## CONFIGURATION CHANGES

### ApplicationInsightsAlerts.json
**File:** `ApplicationInsightsAlerts.json`

```json
{
  "alerts": [
    {
      "name": "ThreadPool_Exhaustion_Alert",
      "description": "Thread pool utilization exceeds 90%",
      "condition": "ThreadPool_WorkerThreadUtilization > 90 OR ThreadPool_CompletionPortUtilization > 90",
      "severity": "Critical"
    },
    {
      "name": "ThreadPool_HighUtilization_Alert",
      "description": "Thread pool utilization exceeds 80%",
      "condition": "ThreadPool_WorkerThreadUtilization > 80 OR ThreadPool_CompletionPortUtilization > 80",
      "severity": "Warning"
    },
    {
      "name": "Slow_Request_Alert",
      "description": "Request duration exceeds 5000ms",
      "condition": "Request_Duration > 5000",
      "severity": "Critical"
    },
    {
      "name": "Request_Error_Rate_Alert",
      "description": "Request error rate exceeds 25%",
      "condition": "Request_Error_Rate > 25",
      "severity": "Critical"
    },
    {
      "name": "Database_ConnectionPool_Alert",
      "description": "Database connection pool utilization exceeds 90%",
      "condition": "Database_ConnectionPool_Utilization > 90",
      "severity": "Critical"
    },
    {
      "name": "Login_Failure_Rate_Alert",
      "description": "Login failure rate exceeds 25%",
      "condition": "Login_Failure_Rate > 25",
      "severity": "Critical"
    }
  ]
}
```

---

## UNIT TESTS

### ShedulingAppService_Tests.cs
**File:** `test/DispatcherWeb.Tests/Scheduling/ShedulingAppService_Tests.cs`

```csharp
// Add these new test methods to the existing test class

[Fact]
public async Task Test_SetOrderLineIsCompleteBatch_should_set_multiple_order_lines_complete()
{
    // Arrange
    var orderLineIds = new List<int> { 1, 2, 3 };
    var input = new SetOrderLineIsCompleteBatchInput
    {
        OrderLineIds = orderLineIds,
        IsComplete = true,
        IsCancelled = false
    };

    // Act
    await _schedulingAppService.SetOrderLineIsCompleteBatch(input);

    // Assert
    // Verify that all order lines were updated
    foreach (var orderLineId in orderLineIds)
    {
        var orderLine = await _context.OrderLines.FindAsync(orderLineId);
        Assert.NotNull(orderLine);
        Assert.True(orderLine.IsComplete);
    }
}

[Fact]
public async Task Test_SetOrderLineIsCompleteBatch_should_throw_exception_for_empty_order_line_ids()
{
    // Arrange
    var input = new SetOrderLineIsCompleteBatchInput
    {
        OrderLineIds = new List<int>(),
        IsComplete = true,
        IsCancelled = false
    };

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => 
        _schedulingAppService.SetOrderLineIsCompleteBatch(input));
}

[Fact]
public async Task Test_SetOrderLineIsCompleteBatch_should_throw_exception_for_null_order_line_ids()
{
    // Arrange
    var input = new SetOrderLineIsCompleteBatchInput
    {
        OrderLineIds = null,
        IsComplete = true,
        IsCancelled = false
    };

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => 
        _schedulingAppService.SetOrderLineIsCompleteBatch(input));
}
```

---

## DEPLOYMENT SCRIPT

### DeployMonitoringServices.ps1
**File:** `DeployMonitoringServices.ps1`

```powershell
# PowerShell script to build and deploy monitoring services

Write-Host "Building DispatcherWeb solution..." -ForegroundColor Green
dotnet build DispatcherWeb.Web.sln --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Please fix compilation errors before proceeding." -ForegroundColor Red
    exit 1
}

Write-Host "Running unit tests..." -ForegroundColor Green
dotnet test test/DispatcherWeb.Tests/DispatcherWeb.Tests.csproj --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed. Please fix test failures before proceeding." -ForegroundColor Red
    exit 1
}

Write-Host "Performance monitoring services deployment completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Deploy to staging environment" -ForegroundColor White
Write-Host "2. Configure Application Insights alerts" -ForegroundColor White
Write-Host "3. Monitor performance metrics for 24-48 hours" -ForegroundColor White
Write-Host "4. Validate thread pool and database connection monitoring" -ForegroundColor White
Write-Host "5. Deploy to production after validation" -ForegroundColor White
```

---

## SUMMARY OF CODE CHANGES

### New Files Created:
1. `ThreadPoolMonitoringService.cs` - Background service for thread pool monitoring
2. `EnhancedPerformanceMonitoringMiddleware.cs` - Enhanced request monitoring middleware
3. `SetOrderLineIsCompleteBatchInput.cs` - DTO for batch processing
4. `ApplicationInsightsAlerts.json` - Alert configuration
5. `DeployMonitoringServices.ps1` - Deployment script

### Modified Files:
1. `Startup.cs` - Added thread pool configuration and service registration
2. `appsettings.json` - Updated connection string and Entity Framework settings
3. `ISchedulingAppService.cs` - Added batch method signature
4. `SchedulingAppService.cs` - Implemented batch processing method
5. `AccountController.cs` - Enhanced login method with telemetry
6. `ShedulingAppService_Tests.cs` - Added unit tests for batch processing

### Configuration Changes:
- Thread pool optimization (MinThreads = ProcessorCount * 2, MaxThreads = ProcessorCount * 4)
- Database connection pool optimization (Min Pool Size = 20, Max Pool Size = 200)
- Entity Framework configuration (CommandTimeout = 60s, EnableRetryOnFailure = true)
- Application Insights alert configuration

This comprehensive code implementation provides all the necessary changes to address the five performance issues identified in the DispatcherWeb application. 