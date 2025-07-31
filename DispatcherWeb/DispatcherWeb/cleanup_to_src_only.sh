#!/bin/bash

echo "=== Cleaning Repository to Keep Only /src Folder ==="
echo "This will remove everything except:"
echo "- /src folder (source code)"
echo "- Essential Git files (.git, .gitignore, .gitattributes)"
echo "- README.md"
echo ""

# Files and folders to remove
FILES_TO_REMOVE=(
    "AspNetZeroRadTool"
    "backup_to_thumb_drive.sh"
    "build"
    "cleanup_repository.ps1"
    "cleanup_repository.sh"
    "Client_Implementation_Analysis.md"
    "Common abbreviations - Overview.pdf"
    "Common issues - Overview.pdf"
    "common.props"
    "Complete Utility Token Management Platform - Technical Specification.pdf"
    "Delete-BIN-OBJ-Folders.bat"
    "DispatcherWeb.AbpDebug.sln"
    "DispatcherWeb.Web.sln"
    "docker"
    ".editorconfig"
    "global.json"
    "Good Developers Do This - Overview.pdf"
    "Implementation_Action_Plan.md"
    "Performance_Optimization_Strategic_Plan.md"
    "Prod deployment - Overview.pdf"
    "Rider specific issues - Overview.pdf"
    "Running locally - Overview.pdf"
    "SetOrderLineIsCompleteBatch_Analysis.md"
    "SetOrderLineIsCompleteBatch_Corrected_Response.md"
    "test"
    "Test.Base"
    ".tfignore"
    "Validation - Overview.pdf"
)

echo "Removing files and folders..."
for item in "${FILES_TO_REMOVE[@]}"; do
    if [ -e "$item" ]; then
        echo "Removing: $item"
        rm -rf "$item"
    fi
done

echo ""
echo "=== Repository Cleanup Completed ==="
echo ""
echo "Remaining structure:"
echo "- /src (source code with all performance optimizations)"
echo "- .git (Git repository)"
echo "- .gitignore"
echo "- .gitattributes"
echo "- README.md"
echo ""
echo "Ready to commit and push changes." 