#!/bin/bash

# Backup script to copy the entire DispatcherWeb project to thumb drive
# Thumb drive is mounted at /media/temporary/005A-2286

BACKUP_SOURCE="/home/temporary/Freelancer/Dump Truck Dispatcher/DispatcherWeb/DispatcherWeb"
BACKUP_DEST="/media/temporary/005A-2286/DispatcherWeb_Backup_$(date +%Y%m%d_%H%M%S)"

echo "=== DispatcherWeb Project Backup Script ==="
echo "Source: $BACKUP_SOURCE"
echo "Destination: $BACKUP_DEST"
echo ""

# Check if source directory exists
if [ ! -d "$BACKUP_SOURCE" ]; then
    echo "ERROR: Source directory does not exist: $BACKUP_SOURCE"
    exit 1
fi

# Check if thumb drive is mounted
if [ ! -d "/media/temporary/005A-2286" ]; then
    echo "ERROR: Thumb drive not found at /media/temporary/005A-2286"
    echo "Please ensure the thumb drive is properly inserted and mounted."
    exit 1
fi

# Check available space on thumb drive
THUMB_DRIVE_SPACE=$(df /media/temporary/005A-2286 | tail -1 | awk '{print $4}')
PROJECT_SIZE=$(du -s "$BACKUP_SOURCE" | awk '{print $1}')

echo "Thumb drive available space: ${THUMB_DRIVE_SPACE}KB"
echo "Project size: ${PROJECT_SIZE}KB"

if [ "$PROJECT_SIZE" -gt "$THUMB_DRIVE_SPACE" ]; then
    echo "ERROR: Not enough space on thumb drive!"
    echo "Project size: ${PROJECT_SIZE}KB"
    echo "Available space: ${THUMB_DRIVE_SPACE}KB"
    exit 1
fi

echo ""
echo "Starting backup..."
echo "This may take several minutes depending on the project size..."
echo ""

# Create backup directory
mkdir -p "$BACKUP_DEST"

# Copy the entire project with progress
echo "Copying project files..."
rsync -av --progress "$BACKUP_SOURCE/" "$BACKUP_DEST/"

if [ $? -eq 0 ]; then
    echo ""
    echo "=== BACKUP COMPLETED SUCCESSFULLY ==="
    echo "Backup location: $BACKUP_DEST"
    echo ""
    echo "Backup contents:"
    ls -la "$BACKUP_DEST"
    echo ""
    echo "Backup size:"
    du -sh "$BACKUP_DEST"
    echo ""
    echo "You can now safely proceed with repository cleanup."
else
    echo ""
    echo "ERROR: Backup failed!"
    echo "Please check the error messages above and try again."
    exit 1
fi 