@echo off
setlocal enabledelayedexpansion

set CONFIG=%~1
if "%CONFIG%"=="" set CONFIG=Release

echo ===================================================
echo Building Linux Installer Package (%CONFIG%)
echo ===================================================

echo.
echo Step 1: Building AIEngineConnectivity...
call build.cmd connectivity %CONFIG%
if !errorlevel! neq 0 (
    echo Failed to build AIEngineConnectivity.
    exit /b !errorlevel!
)

echo.
echo Step 2: Building AIEngineCore...
call build.cmd core %CONFIG%
if !errorlevel! neq 0 (
    echo Failed to build AIEngineCore.
    exit /b !errorlevel!
)

echo.
echo Step 3: Building AIEngineClient (Angular Web App)...
call build.cmd client
if !errorlevel! neq 0 (
    echo Failed to build AIEngineClient.
    exit /b !errorlevel!
)

echo.
echo Step 4: Publishing AIEngineGateway for Linux (linux-x64)...
dotnet publish AIEngineGateway\AIEngineGateway.csproj -c %CONFIG% -r linux-x64 -o "linux-installer/AIEngineGateway"
if !errorlevel! neq 0 (
    echo Failed to publish AIEngineGateway for Linux.
    exit /b !errorlevel!
)

echo.
echo Step 5: Publishing AIEngineInstaller for Linux (linux-x64)...
dotnet publish AIEngineInstaller\AIEngineInstaller.csproj -c %CONFIG% -r linux-x64 -o "linux-installer"
if !errorlevel! neq 0 (
    echo Failed to publish AIEngineInstaller for Linux.
    exit /b !errorlevel!
)

echo.
echo ===================================================
echo Linux Installer package ready in .\linux-installer\
echo ===================================================

endlocal
exit /b 0
