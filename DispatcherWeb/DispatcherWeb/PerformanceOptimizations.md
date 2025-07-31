# Performance Optimizations for DispatcherWeb

## Overview

This document outlines the performance optimizations implemented to resolve the performance hangs identified in the DispatcherWeb application. The optimizations focus on the two main performance bottlenecks: `SetOrderLineIsComplete` and `Account/Login`.

## Implemented Optimizations

### 1. SetOrderLineIsComplete Performance Optimization

#### Problem
- The original `SetOrderLineIsComplete` method was processing order lines individually
- `SetAllOrderLinesIsComplete` was calling the individual method in a loop, causing N+1 query problems
- Each order line update triggered separate database operations

#### Solution
- **Batch Processing**: Created `SetOrderLineIsCompleteBatch` method that processes multiple order lines in a single operation
- **Reduced Database Calls**: Batch updates and deletes instead of individual operations
- **Enhanced Telemetry**: Added detailed performance tracking with Application Insights

#### Key Changes

**New Batch Method:**
```csharp
[AbpAuthorize(AppPermissions.Pages_Schedule, AppPermissions.LeaseHaulerPortal_Schedule)]
public async Task SetOrderLineIsCompleteBatch(SetOrderLineIsCompleteBatchInput input)
{
    // Batch validation and permissions
    // Batch order line updates
    // Batch order line truck operations
    // Single save operation
}
```

**Enhanced Telemetry:**
```csharp
var telemetry = new TelemetryClient();
telemetry.TrackMetric("SetOrderLineIsComplete_Duration", duration);
telemetry.TrackEvent("SetOrderLineIsComplete_Success", new Dictionary<string, string>
{
    { "OrderLineId", input.OrderLineId.ToString() },
    { "IsComplete", input.IsComplete.ToString() },
    { "IsCancelled", input.IsCancelled.ToString() }
});
```

**Optimized SetAllOrderLinesIsComplete:**
```csharp
// Before: Individual calls in loop
foreach (var item in items)
{
    await SetOrderLineIsComplete(new SetOrderLineIsCompleteInput { ... });
}

// After: Single batch call
await SetOrderLineIsCompleteBatch(new SetOrderLineIsCompleteBatchInput
{
    OrderLineIds = items.Select(x => x.Id).ToList(),
    IsComplete = true,
    IsCancelled = false,
});
```

#### Performance Impact
- **Database Operations**: Reduced from N individual operations to 1 batch operation
- **Query Count**: Reduced from O(N) to O(1) for bulk operations
- **Transaction Scope**: Single transaction for all related operations

### 2. Account/Login Performance Enhancement

#### Problem
- Login operations lacked detailed performance monitoring
- No caching for authentication results
- Limited visibility into login performance patterns

#### Solution
- **Enhanced Telemetry**: Added comprehensive performance tracking for all login scenarios
- **Detailed Metrics**: Track success, failure, password change, and two-factor authentication flows
- **Error Tracking**: Enhanced exception handling with performance context

#### Key Changes

**Enhanced Login Method:**
```csharp
public virtual async Task<JsonResult> Login(LoginViewModel loginModel, string returnUrl = "", string returnUrlHash = "", string ss = "")
{
    var startTime = DateTime.UtcNow;
    var telemetry = new TelemetryClient();
    
    try
    {
        // Login logic...
        
        var successDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        telemetry.TrackMetric("Account_Login_Success_Duration", successDuration);
        telemetry.TrackEvent("Account_Login_Success", new Dictionary<string, string>
        {
            { "UserId", loginResult.User.Id.ToString() },
            { "TenantId", loginResult.User.TenantId?.ToString() ?? "null" },
            { "RememberMe", loginModel.RememberMe.ToString() },
            { "SingleSignIn", ss }
        });
    }
    catch (Exception ex)
    {
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        telemetry.TrackException(ex);
        telemetry.TrackMetric("Account_Login_Error_Duration", duration);
        // Error handling...
    }
}
```

#### Performance Impact
- **Monitoring**: Real-time visibility into login performance
- **Alerting**: Automatic detection of slow login operations
- **Debugging**: Detailed context for performance issues

### 3. Performance Monitoring Middleware

#### Problem
- Limited visibility into overall application performance
- No automatic detection of slow requests
- Missing performance context for debugging

#### Solution
- **Request Monitoring**: Automatic tracking of all HTTP requests
- **Slow Request Detection**: Alerts for requests taking > 1000ms
- **Performance Metrics**: Detailed timing and error tracking

#### Implementation

**PerformanceMonitoringMiddleware:**
```csharp
public class PerformanceMonitoringMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            
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
            }
        }
        catch (Exception ex)
        {
            // Error tracking...
        }
    }
}
```

#### Performance Impact
- **Visibility**: Real-time monitoring of all requests
- **Alerting**: Automatic detection of performance issues
- **Debugging**: Detailed context for slow operations

## Testing

### Unit Tests Added

1. **SetOrderLineIsCompleteBatch Tests:**
   - `Test_SetOrderLineIsCompleteBatch_should_set_multiple_order_lines_complete`
   - `Test_SetOrderLineIsCompleteBatch_should_throw_exception_for_empty_order_line_ids`
   - `Test_SetOrderLineIsCompleteBatch_should_throw_exception_for_null_order_line_ids`

2. **Performance Validation:**
   - Batch operations vs individual operations
   - Database query count reduction
   - Transaction scope optimization

## Configuration

### Application Settings

Add to `appsettings.json`:
```json
{
  "App": {
    "DisablePerformanceMonitoringMiddleware": false,
    "DisableAppInsights": false
  }
}
```

### Middleware Registration

The performance monitoring middleware is automatically registered in `Startup.cs`:
```csharp
if (!_appConfiguration.GetValue<bool>("App:DisablePerformanceMonitoringMiddleware")
    && !_appConfiguration.GetValue<bool>("App:DisableAppInsights"))
{
    app.UseMiddleware<PerformanceMonitoringMiddleware>();
}
```

## Monitoring and Alerts

### Application Insights Metrics

1. **SetOrderLineIsComplete Metrics:**
   - `SetOrderLineIsComplete_Duration`
   - `SetOrderLineIsComplete_Success`
   - `SetOrderLineIsComplete_Error_Duration`

2. **Batch Operation Metrics:**
   - `SetOrderLineIsCompleteBatch_Duration`
   - `SetOrderLineIsCompleteBatch_Success`
   - `SetOrderLineIsCompleteBatch_Error_Duration`

3. **Login Metrics:**
   - `Account_Login_Success_Duration`
   - `Account_Login_Error_Duration`
   - `Account_Login_PasswordChangeRequired_Duration`
   - `Account_Login_TwoFactorRequired_Duration`

4. **Request Monitoring:**
   - `Request_Duration`
   - `SlowRequest_Duration`
   - `Request_Error_Duration`

### Recommended Alerts

1. **Slow Request Alert:**
   - Metric: `SlowRequest_Duration`
   - Threshold: > 5000ms
   - Action: Email notification

2. **Login Performance Alert:**
   - Metric: `Account_Login_Success_Duration`
   - Threshold: > 2000ms
   - Action: Email notification

3. **Batch Operation Alert:**
   - Metric: `SetOrderLineIsCompleteBatch_Duration`
   - Threshold: > 10000ms
   - Action: Email notification

## Deployment Notes

### Prerequisites
- Application Insights configured
- Database connection optimized
- Monitoring alerts configured

### Rollback Plan
1. Disable performance monitoring middleware: `"DisablePerformanceMonitoringMiddleware": true`
2. Revert to individual `SetOrderLineIsComplete` calls if needed
3. Monitor Application Insights for any issues

### Performance Validation
1. Run load tests comparing old vs new implementations
2. Monitor database query counts
3. Validate Application Insights metrics
4. Check for any regression in functionality

## Future Optimizations

### Potential Improvements
1. **Caching Layer:**
   - Cache frequently accessed order line data
   - Implement Redis for session management
   - Cache ABP service proxies

2. **Database Optimizations:**
   - Add database indexes for frequently queried columns
   - Optimize stored procedures
   - Implement read replicas for heavy read operations

3. **Async Processing:**
   - Move heavy operations to background jobs
   - Implement queue-based processing for bulk operations
   - Use Hangfire for scheduled performance-intensive tasks

### Monitoring Enhancements
1. **Custom Dashboards:**
   - Create Application Insights dashboards for performance metrics
   - Set up automated performance reports
   - Implement real-time performance monitoring

2. **Advanced Alerting:**
   - Machine learning-based anomaly detection
   - Predictive performance alerts
   - Business impact correlation

## Conclusion

These optimizations provide:
- **Immediate Performance Gains**: Batch processing reduces database load
- **Better Visibility**: Comprehensive monitoring enables proactive issue detection
- **Scalability**: Optimized operations handle increased load better
- **Maintainability**: Enhanced telemetry aids in debugging and optimization

The implementation follows the 4-day plan outlined in the performance fix plan, focusing on the most critical performance bottlenecks while maintaining system stability and functionality. 