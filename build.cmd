@echo off
setlocal enabledelayedexpansion
set TARGET=%~1
if "%TARGET%"=="" set TARGET=all
set CONFIG=%~2
if "%CONFIG%"=="" set CONFIG=Debug
if /I not "%TARGET%"=="all" if /I not "%TARGET%"=="connectivity" if /I not "%TARGET%"=="core" if /I not "%TARGET%"=="speechtotext" if /I not "%TARGET%"=="gateway" if /I not "%TARGET%"=="client" if /I not "%TARGET%"=="installer" if /I not "%TARGET%"=="windowsinstaller" if /I not "%TARGET%"=="linuxinstaller"  (
    echo Invalid target. Use: all, connectivity, core, speechtotext, gateway, client, installer, windowsinstaller, linuxinstaller
    echo Usage: build.cmd [target] [configuration]
    echo Default: build.cmd all Release
    exit /b 1
)
if /I not "%CONFIG%"=="Debug" if /I not "%CONFIG%"=="Release" (
    echo Invalid configuration. Use: Debug, Release
    echo Usage: build.cmd [target] [configuration]
    echo Default: build.cmd all Release
    exit /b 1
)
set CONNECTIVITY_SLN=AIEngineConnectivity\AIEngineConnectivity.slnx
set CORE_SLN=AIEngineCore\AIEngineCore.slnx
set GATEWAY_SLN=AIEngineGateway\AIEngineGateway.slnx
set SPEECH_SLN=AIEngineSpeechRecognition\AIEngineSpeechRecognition.slnx
set INSTALLER_SLN=AIEngineInstaller\AIEngineInstaller.slnx
if /I "%TARGET%"=="all" goto build_all
if /I "%TARGET%"=="connectivity" goto build_connectivity
if /I "%TARGET%"=="core" goto build_core
if /I "%TARGET%"=="speechtotext" goto build_speech
if /I "%TARGET%"=="gateway" goto build_gateway
if /I "%TARGET%"=="client" goto build_client
if /I "%TARGET%"=="installer" goto build_installer
if /I "%TARGET%"=="windowsinstaller" goto build_windows_installer
if /I "%TARGET%"=="linuxinstaller" goto build_linux_installer

:build_all
call :build_connectivity
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_core
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_installer
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_speech
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_gateway
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_client
if !errorlevel! neq 0 exit /b !errorlevel!
echo Build completed successfully for target: all
goto end

:build_connectivity
echo.
echo Building AIEngineConnectivity (%CONFIG%)...
dotnet build "%CONNECTIVITY_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!

echo.
echo Copying AIEngineConnectivity outputs to AIEngineCore\libs....
if not exist "AIEngineCore\libs" mkdir "AIEngineCore\libs"
xcopy /Y /S /I "AIEngineConnectivity\bin\%CONFIG%\net10.0\*" "AIEngineCore\libs\"
echo Copying AIEngineConnectivity outputs to AIEngineSpeechRecognition\libs....
if not exist "AIEngineSpeechRecognition\libs" mkdir "AIEngineSpeechRecognition\libs"
xcopy /Y /S /I "AIEngineConnectivity\bin\%CONFIG%\net10.0\*" "AIEngineSpeechRecognition\libs\"
echo Copying AIEngineConnectivity outputs to AIEngineGateway\libs...
if not exist "AIEngineGateway\libs" mkdir "AIEngineGateway\libs"
xcopy /Y /S /I "AIEngineConnectivity\bin\%CONFIG%\net10.0\*" "AIEngineGateway\libs\"
echo Copying AIEngineConnectivity outputs to AIEngineInstaller\libs...
if not exist "AIEngineInstaller\libs" mkdir "AIEngineInstaller\libs"
xcopy /Y /S /I "AIEngineConnectivity\bin\%CONFIG%\net10.0\*" "AIEngineInstaller\libs\"
if !errorlevel! neq 0 exit /b !errorlevel!
if /I "%TARGET%"=="connectivity" (
    echo Build completed successfully for target: connectivity
)
goto :eof

:build_core
echo.
echo Building AIEngineCore (%CONFIG%)...
dotnet build "%CORE_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!

echo.
echo Copying AIEngineCore outputs to AIEngineGateway\libs...
if not exist "AIEngineGateway\libs" mkdir "AIEngineGateway\libs"
xcopy /Y /S /I "AIEngineCore\bin\%CONFIG%\net10.0\*" "AIEngineGateway\libs\"
echo Copying AIEngineCore outputs to AIEngineInstaller\libs...
if not exist "AIEngineInstaller\libs" mkdir "AIEngineInstaller\libs"
xcopy /Y /S /I "AIEngineCore\bin\%CONFIG%\net10.0\*" "AIEngineInstaller\libs\"
if !errorlevel! neq 0 exit /b !errorlevel!
if /I "%TARGET%"=="core" (
    echo Build completed successfully for target: core
)
goto :eof

:build_speech
echo.
echo Building AIEngineSpeechRecognition (%CONFIG%)...
dotnet build "%SPEECH_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!

if /I "%TARGET%"=="speechtotext" (
    echo Build completed successfully for target: speechtotext
)
goto :eof

:build_gateway
echo.
echo Building AIEngineGateway (%CONFIG%)...
dotnet build "%GATEWAY_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
if /I "%TARGET%"=="gateway" (
    echo Build completed successfully for target: gateway
)
goto :eof

:build_client
echo.
echo Building AIEngineClient...
pushd AIEngineClient
call npm run build
if !errorlevel! neq 0 (
    popd
    exit /b !errorlevel!
)
popd
if /I "%TARGET%"=="client" (
    echo Build completed successfully for target: client
)
goto :eof

:build_installer
echo.
echo Building AIEngineInstaller (%CONFIG%)...
dotnet build "%INSTALLER_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
if /I "%TARGET%"=="installer" (
    echo Build completed successfully for target: installer
)
goto :eof

:build_windows_installer
call :build_connectivity
if !errorlevel! neq 0 exit /b !errorlevel!

call :build_core
if !errorlevel! neq 0 exit /b !errorlevel!

echo.
echo Building AIEngineInstaller (%CONFIG%) for Windows...
dotnet publish "%INSTALLER_SLN%" -c "%CONFIG%" -r win-x64 -o "windows-installer"
if !errorlevel! neq 0 exit /b !errorlevel!
if /I "%TARGET%"=="windowsinstaller" (
    echo Build completed successfully for target: windowsinstaller
)
goto :eof

:build_linux_installer
call :build_connectivity
if !errorlevel! neq 0 exit /b !errorlevel!

call :build_core
if !errorlevel! neq 0 exit /b !errorlevel!

echo.
echo Building AIEngineInstaller (%CONFIG%) for Linux...
dotnet publish "%INSTALLER_SLN%" -c "%CONFIG%" -r linux-x64 -o "linux-installer"
if !errorlevel! neq 0 exit /b !errorlevel!
if /I "%TARGET%"=="linuxinstaller" (
    echo Build completed successfully for target: linuxinstaller
)
goto :eof


:end
endlocal
exit /b 0