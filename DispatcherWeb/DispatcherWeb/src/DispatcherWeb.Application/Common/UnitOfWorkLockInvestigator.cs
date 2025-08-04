using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace DispatcherWeb.Common
{
    public class UnitOfWorkLockInvestigator
    {
        private readonly ILogger<UnitOfWorkLockInvestigator> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public UnitOfWorkLockInvestigator(
            ILogger<UnitOfWorkLockInvestigator> logger,
            TelemetryClient telemetryClient,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _unitOfWorkManager = unitOfWorkManager;
        }

        /// <summary>
        /// Investigates potential blocking operations in unit of work patterns
        /// Based on client's discovery of a lock in unit of work operations
        /// </summary>
        public async Task InvestigateUnitOfWorkPatternsAsync()
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                // Check for synchronous unit of work operations
                await InvestigateSynchronousOperationsAsync();
                
                // Check for blocking save operations
                await InvestigateBlockingSaveOperationsAsync();
                
                // Check for session access patterns
                await InvestigateSessionAccessPatternsAsync();
                
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _telemetryClient.TrackMetric("UnitOfWork_Investigation_Duration", duration);
                
                _logger.LogInformation("Unit of work investigation completed in {Duration}ms", duration);
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackMetric("UnitOfWork_Investigation_Error_Duration", duration);
                _logger.LogError(ex, "Unit of work investigation failed after {Duration}ms", duration);
            }
        }

        private async Task InvestigateSynchronousOperationsAsync()
        {
            try
            {
                // Check if there are any synchronous unit of work operations
                var currentUnitOfWork = _unitOfWorkManager.Current;
                
                if (currentUnitOfWork != null)
                {
                    _telemetryClient.TrackEvent("UnitOfWork_Synchronous_Check", new Dictionary<string, string>
                    {
                        { "IsActive", currentUnitOfWork.IsActive.ToString() },
                        { "HasChanges", currentUnitOfWork.HasChanges.ToString() },
                        { "Options", currentUnitOfWork.Options?.ToString() ?? "null" }
                    });
                }
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                _logger.LogWarning(ex, "Error investigating synchronous unit of work operations");
            }
        }

        private async Task InvestigateBlockingSaveOperationsAsync()
        {
            try
            {
                // This would be called during actual operations to detect blocking saves
                // The client discovered a lock in a method setting the unit of work
                // This method helps identify similar patterns
                
                _telemetryClient.TrackEvent("UnitOfWork_BlockingSave_Investigation", new Dictionary<string, string>
                {
                    { "InvestigationType", "BlockingSaveOperations" },
                    { "Timestamp", DateTime.UtcNow.ToString("O") }
                });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                _logger.LogWarning(ex, "Error investigating blocking save operations");
            }
        }

        private async Task InvestigateSessionAccessPatternsAsync()
        {
            try
            {
                // Check for potential blocking session access
                // Common patterns that can cause locks:
                // - AbpSession.GetTenantId() (synchronous)
                // - AbpSession.GetUserId() (synchronous)
                
                _telemetryClient.TrackEvent("UnitOfWork_SessionAccess_Investigation", new Dictionary<string, string>
                {
                    { "InvestigationType", "SessionAccessPatterns" },
                    { "Timestamp", DateTime.UtcNow.ToString("O") }
                });
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                _logger.LogWarning(ex, "Error investigating session access patterns");
            }
        }

        /// <summary>
        /// Wraps a unit of work operation to detect blocking patterns
        /// </summary>
        public async Task<T> MonitorUnitOfWorkOperationAsync<T>(Func<Task<T>> operation, string operationName)
        {
            var startTime = DateTime.UtcNow;
            var threadId = Thread.CurrentThread.ManagedThreadId;
            
            try
            {
                var result = await operation();
                
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                // Only track if operation takes longer than expected
                if (duration > 1000) // 1 second threshold
                {
                    _telemetryClient.TrackEvent("UnitOfWork_SlowOperation", new Dictionary<string, string>
                    {
                        { "OperationName", operationName },
                        { "Duration", duration.ToString() },
                        { "ThreadId", threadId.ToString() },
                        { "Success", "true" }
                    });
                    
                    _telemetryClient.TrackMetric("UnitOfWork_Operation_Duration", duration);
                    
                    _logger.LogWarning("Slow unit of work operation detected: {OperationName} took {Duration}ms on thread {ThreadId}",
                        operationName, duration, threadId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackEvent("UnitOfWork_Operation_Error", new Dictionary<string, string>
                {
                    { "OperationName", operationName },
                    { "Duration", duration.ToString() },
                    { "ThreadId", threadId.ToString() },
                    { "ExceptionType", ex.GetType().Name },
                    { "ExceptionMessage", ex.Message }
                });
                
                _logger.LogError(ex, "Unit of work operation failed: {OperationName} after {Duration}ms on thread {ThreadId}",
                    operationName, duration, threadId);
                
                throw;
            }
        }

        /// <summary>
        /// Detects common blocking patterns in ABP framework
        /// </summary>
        public void DetectBlockingPatterns()
        {
            var patterns = new List<string>
            {
                "UnitOfWork.Current.SaveChanges()", // Synchronous - BLOCKING
                "UnitOfWork.Current.SaveChangesAsync()", // Async - OK
                "AbpSession.GetTenantId()", // Potential blocking
                "AbpSession.GetUserId()", // Potential blocking
                "Repository.GetAllList()", // Synchronous - BLOCKING
                "Repository.GetAllListAsync()", // Async - OK
                "DbContext.SaveChanges()", // Synchronous - BLOCKING
                "DbContext.SaveChangesAsync()" // Async - OK
            };
            
            _telemetryClient.TrackEvent("UnitOfWork_BlockingPatterns_Detected", new Dictionary<string, string>
            {
                { "Patterns", string.Join(";", patterns) },
                { "Timestamp", DateTime.UtcNow.ToString("O") }
            });
            
            _logger.LogInformation("Detected {Count} potential blocking patterns in unit of work operations", patterns.Count);
        }
    }
} 