@echo off
echo ========================================
echo DispatcherWeb Azurite Fix Script
echo ========================================

echo.
echo Step 1: Stopping any existing Azurite processes...
taskkill /f /im azurite.exe 2>nul
if %errorlevel% equ 0 (
    echo ✅ Stopped existing Azurite processes
) else (
    echo ℹ️ No existing Azurite processes found
)

echo.
echo Step 2: Starting Azurite with proper configuration...
start /b azurite --silent --skipApiVersionCheck --location C:\azurite --debug C:\azurite\debug.log

echo.
echo Step 3: Waiting for Azurite to start...
timeout /t 3 /nobreak >nul

echo.
echo Step 4: Checking if Azurite ports are available...
netstat -an | findstr 10000
netstat -an | findstr 10001
netstat -an | findstr 10002

echo.
echo Step 5: Testing Azure Storage connection...
echo You can now test the connection by visiting:
echo http://localhost:44332/test/azure-storage
echo.

echo ========================================
echo If you still get connection errors:
echo 1. Make sure Azurite is installed: npm install -g azurite
echo 2. Try running Azurite manually: azurite --silent
echo 3. Check if ports 10000, 10001, 10002 are free
echo 4. The app is now configured to use file system for data protection
echo ========================================

pause 