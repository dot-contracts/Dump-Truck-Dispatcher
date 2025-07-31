# PowerShell script to clean up the remote repository by removing performance analysis documentation files
# This will keep only the source code and project files

Write-Host "Cleaning up remote repository - removing performance analysis documentation files..." -ForegroundColor Green

# List of files to remove from remote repository
$filesToRemove = @(
    "DispatcherWeb_Performance_Solutions_Complete.md",
    "DispatcherWeb_PerformanceIssues_Solutions.docx.md",
    "Performance_Implementation_Code.md",
    "PerformanceIssuesSolved.txt",
    "ClientPerformanceSolution.md",
    "RefinedPerformanceSolution.md",
    "ThreadPoolStarvationFix.md",
    "PerformanceOptimizations.md",
    "ImplementationSummary.md",
    "PerformanceFixPlan.markdown",
    "PerformanceAnalysis2.docx",
    "Issue1_ThreadPoolStarvation.txt",
    "Issue2_DatabaseConnectionPoolExhaustion.txt",
    "Issue3_N1QueryProblem.txt",
    "Issue4_ComprehensivePerformanceMonitoring.txt",
    "Issue5_AuthenticationPerformance.txt",
    "ApplicationInsightsAlerts.json",
    "DeployMonitoringServices.ps1",
    "PerformanceValidation.cs",
    "fix-azurite-issue.bat",
    "diagnostic-cache-check.cs"
)

# Remove each file from git tracking and remote repository
foreach ($file in $filesToRemove) {
    if (Test-Path $file) {
        Write-Host "Removing $file from repository..." -ForegroundColor Yellow
        git rm $file
    } else {
        Write-Host "File $file not found, skipping..." -ForegroundColor Gray
    }
}

# Commit the changes
Write-Host "Committing removal of documentation files..." -ForegroundColor Green
git commit -m "Clean up repository: Remove performance analysis documentation files

Removed files:
- Performance analysis and documentation files
- Individual issue analysis files
- Utility scripts and configuration files
- Implementation guides and summaries

Kept:
- Source code in /src folder
- Project configuration files
- Original documentation PDFs
- Build and deployment files"

Write-Host "Repository cleanup completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Files removed from remote repository:" -ForegroundColor Cyan
foreach ($file in $filesToRemove) {
    Write-Host "  - $file" -ForegroundColor White
}
Write-Host ""
Write-Host "To push changes to remote repository, run:" -ForegroundColor Yellow
Write-Host "  git push origin main" -ForegroundColor White
Write-Host ""
Write-Host "Note: The source code in /src folder with all performance optimizations remains intact." -ForegroundColor Green 