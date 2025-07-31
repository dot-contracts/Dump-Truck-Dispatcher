# Deploy Monitoring Services Script
# This script helps deploy the performance monitoring services

Write-Host "=== Deploying Performance Monitoring Services ===" -ForegroundColor Green

# Step 1: Build the application
Write-Host "Step 1: Building application..." -ForegroundColor Yellow
dotnet build DispatcherWeb.Web.sln --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Please check for compilation errors." -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green

# Step 2: Run tests to ensure nothing is broken
Write-Host "Step 2: Running tests..." -ForegroundColor Yellow
dotnet test test/DispatcherWeb.Tests/DispatcherWeb.Tests.csproj --configuration Release --no-build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed! Please check for test failures." -ForegroundColor Red
    exit 1
}

Write-Host "Tests completed successfully!" -ForegroundColor Green

# Step 3: Deploy to staging (if configured)
Write-Host "Step 3: Ready for deployment..." -ForegroundColor Yellow
Write-Host "The following monitoring services have been added:" -ForegroundColor Cyan
Write-Host "  ✓ ThreadPoolMonitoringService - Real-time thread pool monitoring" -ForegroundColor Green
Write-Host "  ✓ EnhancedPerformanceMonitoringMiddleware - Comprehensive request monitoring" -ForegroundColor Green
Write-Host "  ✓ Optimized database connection settings" -ForegroundColor Green
Write-Host "  ✓ Thread pool configuration for better performance" -ForegroundColor Green

Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Deploy to your staging environment" -ForegroundColor White
Write-Host "2. Monitor Application Insights for 24-48 hours" -ForegroundColor White
Write-Host "3. Check for the following metrics:" -ForegroundColor White
Write-Host "   - ThreadPool_WorkerThreadUtilization (should stay below 80%)" -ForegroundColor White
Write-Host "   - ThreadPool_CompletionPortUtilization (should stay below 80%)" -ForegroundColor White
Write-Host "   - Request_Duration (should stay below 2000ms)" -ForegroundColor White
Write-Host "4. Set up alerts for thread pool exhaustion" -ForegroundColor White

Write-Host "`nDeployment script completed!" -ForegroundColor Green 