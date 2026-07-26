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
if /I "%TARGET%"=="client" goto build_client
if /I "%TARGET%"=="gateway" goto build_gateway
if /I "%TARGET%"=="installer" goto build_installer

echo Unknown target: %TARGET%
echo Usage: build.cmd [all^|connectivity^|core^|speech^|client^|gateway^|installer] [Release^|Debug]
exit /b 1

:build_all
call :build_connectivity
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_core
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_speech
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_client
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_gateway
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_installer
if !errorlevel! neq 0 exit /b !errorlevel!
echo Build completed successfully for target: all
goto end

:build_connectivity
echo.
echo Building AIEngineConnectivity (%CONFIG%)...
dotnet build "%CONNECTIVITY_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
echo Updating AIEngineConnectivity references in dependent solutions...
if not exist "AIEngineGateway\libs" mkdir "AIEngineGateway\libs"
if not exist "AIEngineInstaller\libs" mkdir "AIEngineInstaller\libs"
copy /Y "AIEngineConnectivity\bin\%CONFIG%\net10.0\AIEngineConnectivity.*" "AIEngineGateway\libs\"
copy /Y "AIEngineConnectivity\bin\%CONFIG%\net10.0\AIEngineConnectivity.*" "AIEngineInstaller\libs\"
goto :eof

:build_core
echo.
echo Building AIEngineCore (%CONFIG%)...
dotnet build "%CORE_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
echo Updating AIEngineCore references in dependent solutions...
if not exist "AIEngineGateway\libs" mkdir "AIEngineGateway\libs"
if not exist "AIEngineInstaller\libs" mkdir "AIEngineInstaller\libs"
copy /Y "AIEngineCore\bin\%CONFIG%\net10.0\AIEngineCore.*" "AIEngineGateway\libs\"
copy /Y "AIEngineCore\bin\%CONFIG%\net10.0\AIEngineCore.*" "AIEngineInstaller\libs\"
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
echo Building AIEngineGateway (%CONFIG%)...
dotnet build "%GATEWAY_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
goto :eof

:build_installer
echo.
echo Building AIEngineInstaller (%CONFIG%)...
dotnet build "%INSTALLER_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
goto :eof

:end
endlocal
exit /b 0