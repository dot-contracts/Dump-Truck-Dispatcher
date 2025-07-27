# 4-Day Plan to Resolve Performance Hangs in DispatcherWeb

## Day 1: Setup, Initial Diagnostics, and Data Collection (8-10 hours)

### Tasks

1. **Set Up Local Environment (2-3 hours)**:
   - Install Visual Studio 2022, SQL Server 2019, Azurite, yarn, SSMS.
   - Clone `DdtAbp` repo, open `DispatcherWeb.Web.sln`, set `DispatcherWeb.Web.Mvc` as startup with IIS Express.
   - Create/restore `DispatcherWebDb` with multi-tenant data.
   - Set `App.SeedHostStartUp` to `true`, run `yarn create-bundles`, start Azurite.
   - Log in (admin/123qwe), add test tenant.
   - Run code cleanup (Profile 1), verify no Git changes, run unit tests.
   - Install `dotnet-counters`, `dotnet-trace`.
2. **Instrument Diagnostics (3-4 hours)**:
   - Add Application Insights telemetry for `SetOrderLineIsComplete`, `Account/Login`:

     ```csharp
     var telemetry = new TelemetryClient();
     var startTime = DateTime.UtcNow;
     // Method execution
     telemetry.TrackMetric("MethodName_Duration", (DateTime.UtcNow - startTime).TotalMilliseconds);
     ```
   - Monitor SQL Server locks and slow queries:

     ```sql
     SELECT resource_type, request_mode, request_status FROM sys.dm_tran_locks WHERE resource_database_id = DB_ID('DispatcherWebDb');
     SELECT TOP 10 st.text, qs.execution_count, qs.total_elapsed_time / 1000 AS total_ms
     FROM sys.dm_exec_query_stats qs CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
     WHERE st.dbid = DB_ID('DispatcherWebDb') ORDER BY qs.total_elapsed_time DESC;
     ```
   - Monitor .NET thread pool: `dotnet-counters monitor --process-id <pid> --counters System.Runtime`.
3. **Initial Data Collection (2-3 hours)**:
   - Review Application Insights, Azure SQL logs, Hangfire dashboard for production patterns.
   - Run JMeter load tests locally (11k/day `Account/Login`, &gt;2000/day `SetOrderLineIsComplete`).
   - Hypothesize causes (e.g., database locks, async issues, excessive logins).

### Deliverables

- Local app running, clean codebase, passing tests.
- Telemetry and monitoring scripts active.
- Initial findings (e.g., “Locks detected in `SetOrderLineIsComplete`”).

## Day 2: Deep Diagnostics and First Fix (8-10 hours)

### Tasks

1. **Deep Diagnostics (3-4 hours)**:
   - Correlate production hang timestamps with traces (e.g., `Account/Login` spikes).
   - Profile locally with `dotnet-trace collect --process-id <pid>`.
   - Inspect `SetOrderLineIsComplete` for queries in loops, `Account/Login` for token issues.
2. **Optimize** `SetOrderLineIsComplete` **(3-4 hours)**:
   - Batch updates:

     ```csharp
     public async Task SetOrderLineIsCompleteAsync(IEnumerable<int> orderLineIds, bool isComplete)
     {
         var telemetry = new TelemetryClient();
         var startTime = DateTime.UtcNow;
         try
         {
             var orderLines = await _context.OrderLines.Where(ol => orderLineIds.Contains(ol.Id)).ToListAsync();
             orderLines.ForEach(ol => ol.IsComplete = isComplete);
             _context.OrderLines.UpdateRange(orderLines);
             await _context.SaveChangesAsync();
             telemetry.TrackMetric("SetOrderLineIsComplete_Success", (DateTime.UtcNow - startTime).TotalMilliseconds);
         }
         catch (Exception ex)
         {
             telemetry.TrackException(ex);
             throw;
         }
     }
     ```
   - Test with JMeter, run unit tests, commit: `#<taskId> Batched SetOrderLineIsComplete`.
3. **Validate and Document (1-2 hours)**:
   - Confirm reduced queries/locks in load tests.
   - Draft findings for team, submit PR.

### Deliverables

- Root cause hypothesis (e.g., “Hangs due to per-row updates”).
- Optimized `SetOrderLineIsComplete`, PR submitted.

## Day 3: Additional Fixes and Testing (8-10 hours)

### Tasks

1. **Optimize** `Account/Login` **(3-4 hours)**:
   - Extend JWT lifetime:

     ```json
     {
       "Jwt": {
         "Expiration": "24:00:00"
       }
     }
     ```
   - Cache authentication:

     ```csharp
     public async Task<AuthResult> LoginAsync(string userId)
     {
         var cacheKey = $"Auth_{userId}";
         var authResult = await _cache.GetOrCreateAsync(cacheKey, async entry =>
         {
             entry.SlidingExpiration = TimeSpan.FromMinutes(30);
             var result = await AuthenticateUserAsync(userId);
             return result;
         });
         _telemetryClient.TrackEvent("LoginAttempt", new Dictionary<string, string> { { "Cached", authResult.IsFromCache ? "True" : "False" } });
         return authResult;
     }
     ```
   - Test with JMeter, commit: `#<taskId> Cached Account/Login and extended JWT lifetime`.
2. **Cache ABP Proxies/Scripts (2-3 hours)**:
   - Cache `AbpServiceProxies/GetAll` and `AbpScripts/GetScripts`:

     ```csharp
     public async Task<List<ServiceProxy>> GetAllServiceProxiesAsync()
     {
         var cacheKey = "AbpServiceProxies_All";
         return await _cache.GetOrCreateAsync(cacheKey, async entry =>
         {
             entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
             return await _abpProxyRepository.GetAllAsync();
         });
     }
     ```
   - Test cache hit rates, commit: `#<taskId> Cached AbpServiceProxies/GetAll and AbpScripts/GetScripts`.
3. **Multi-Tenant Testing (2-3 hours)**:
   - Test fixes across tenants, simulate RN Driver App syncs.
   - Monitor telemetry for improvements.

### Deliverables

- Optimized `Account/Login` and ABP caching, PRs submitted.
- Multi-tenant test results.

## Day 4: Validation, Documentation, and Handover (8-10 hours)

### Tasks

1. **Staging Validation (3-4 hours)**:
   - Deploy to `qa4`, run load tests, monitor telemetry.
   - Address validation issues (e.g., connected dates in `UpdateOrderLineDates`).
2. **Finalize PRs (2-3 hours)**:
   - Address feedback, run final cleanup/tests.
   - Ensure atomic commits and separate PRs for improvements.
3. **Document and Handover (2-3 hours)**:
   - Write report: root cause, fixes, metrics, next steps.
   - Post in Teams or email, request deployment and follow-up meeting.

### Deliverables

- Staging test results, finalized PRs, comprehensive report.