#!/bin/bash

# Script to clean up the remote repository by removing performance analysis documentation files
# This will keep only the source code and project files

echo "Cleaning up remote repository - removing performance analysis documentation files..."

# List of files to remove from remote repository
files_to_remove=(
    "DispatcherWeb_Performance_Solutions_Complete.md"
    "DispatcherWeb_PerformanceIssues_Solutions.docx.md"
    "Performance_Implementation_Code.md"
    "PerformanceIssuesSolved.txt"
    "ClientPerformanceSolution.md"
    "RefinedPerformanceSolution.md"
    "ThreadPoolStarvationFix.md"
    "PerformanceOptimizations.md"
    "ImplementationSummary.md"
    "PerformanceFixPlan.markdown"
    "PerformanceAnalysis2.docx"
    "Issue1_ThreadPoolStarvation.txt"
    "Issue2_DatabaseConnectionPoolExhaustion.txt"
    "Issue3_N1QueryProblem.txt"
    "Issue4_ComprehensivePerformanceMonitoring.txt"
    "Issue5_AuthenticationPerformance.txt"
    "ApplicationInsightsAlerts.json"
    "DeployMonitoringServices.ps1"
    "PerformanceValidation.cs"
    "fix-azurite-issue.bat"
    "diagnostic-cache-check.cs"
)

# Remove each file from git tracking and remote repository
for file in "${files_to_remove[@]}"; do
    if [ -f "$file" ]; then
        echo "Removing $file from repository..."
        git rm "$file"
    else
        echo "File $file not found, skipping..."
    fi
done

# Commit the changes
echo "Committing removal of documentation files..."
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

echo "Repository cleanup completed!"
echo ""
echo "Files removed from remote repository:"
for file in "${files_to_remove[@]}"; do
    echo "  - $file"
done
echo ""
echo "To push changes to remote repository, run:"
echo "  git push origin main"
echo ""
echo "Note: The source code in /src folder with all performance optimizations remains intact." 