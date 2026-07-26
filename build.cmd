@echo off
setlocal enabledelayedexpansion
set TARGET=%~1
if "%TARGET%"=="" set TARGET=all
set CONFIG=%~2
if "%CONFIG%"=="" set CONFIG=Debug

set CONNECTIVITY_SLN=AIEngineConnectivity\AIEngineConnectivity.slnx
set CORE_SLN=AIEngineCore\AIEngineCore.slnx
set GATEWAY_SLN=AIEngineGateway\AIEngineGateway.slnx
set GATEWAY_PROJ=AIEngineGateway\AIEngineGateway.csproj
set SPEECH_SLN=AIEngineSpeechRecognition\AIEngineSpeechRecognition.slnx
set INSTALLER_SLN=AIEngineInstaller\AIEngineInstaller.slnx
set INSTALLER_PROJ=AIEngineInstaller\AIEngineInstaller.csproj

if /I "%TARGET%"=="all" goto build_all
if /I "%TARGET%"=="connectivity" goto build_connectivity
if /I "%TARGET%"=="core" goto build_core
if /I "%TARGET%"=="speech" goto build_speech
if /I "%TARGET%"=="windowsinstaller" goto build_windows_installer
if /I "%TARGET%"=="linuxinstaller" goto build_linux_installer
if /I "%TARGET%"=="gateway" goto build_gateway
if /I "%TARGET%"=="client" goto build_client
if /I "%TARGET%"=="installer" goto build_installer

echo Unknown target: %TARGET%
echo Usage: build.cmd [all^|connectivity^|core^|speech^|client^|gateway^|installer^|windowsinstaller^|linuxinstaller] [Release^|Debug]
exit /b 1

:build_all
call :build_connectivity
call :build_core
call :build_speech
call :build_client
call :build_gateway
call :build_installer
echo Build completed successfully for target: all
goto end

:build_connectivity
echo.
echo Building AIEngineConnectivity (%CONFIG%)...
dotnet build "%CONNECTIVITY_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
goto :eof

:build_core
echo.
echo Building AIEngineCore (%CONFIG%)...
dotnet build "%CORE_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
goto :eof

:build_speech
echo.
echo Building AIEngineSpeechRecognition (%CONFIG%)...
dotnet build "%SPEECH_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
goto :eof

:build_client
echo.
echo Building AIEngineClient Angular Web App...
pushd AIEngineClient
call npm run build
if !errorlevel! neq 0 (
    popd
    exit /b !errorlevel!
)
popd
goto :eof

:build_gateway
echo.
echo Publishing AIEngineGateway (%CONFIG%)...
dotnet publish "%GATEWAY_PROJ%" -c "%CONFIG%" -o "AIEngineGateway\bin\publish"
if !errorlevel! neq 0 exit /b !errorlevel!
goto :eof

:build_installer
echo.
echo Building AIEngineInstaller (%CONFIG%)...
dotnet build "%INSTALLER_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
goto :eof

:build_windows_installer
call :build_connectivity
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_core
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_client
if !errorlevel! neq 0 exit /b !errorlevel!

echo.
echo Publishing AIEngineGateway for Windows...
dotnet publish "%GATEWAY_PROJ%" -c "%CONFIG%" -r win-x64 -o "windows-installer\AIEngineGateway"
if !errorlevel! neq 0 exit /b !errorlevel!

echo.
echo Publishing AIEngineInstaller for Windows...
dotnet publish "%INSTALLER_PROJ%" -c "%CONFIG%" -r win-x64 -o "windows-installer"
if !errorlevel! neq 0 exit /b !errorlevel!
echo Windows Installer build completed successfully!
goto :eof

:build_linux_installer
call :build_connectivity
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_core
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_client
if !errorlevel! neq 0 exit /b !errorlevel!

echo.
echo Publishing AIEngineGateway for Linux...
dotnet publish "%GATEWAY_PROJ%" -c "%CONFIG%" -r linux-x64 -o "linux-installer/AIEngineGateway"
if !errorlevel! neq 0 exit /b !errorlevel!

echo.
echo Publishing AIEngineInstaller for Linux...
dotnet publish "%INSTALLER_PROJ%" -c "%CONFIG%" -r linux-x64 -o "linux-installer"
if !errorlevel! neq 0 exit /b !errorlevel!
echo Linux Installer build completed successfully!
goto :eof

:end
endlocal
exit /b 0