@echo off
setlocal EnableExtensions
title Crystrap Publish
color 0B

set "ROOT=%~dp0"
set "PROJECT=%ROOT%Bloxstrap\Bloxstrap.csproj"
set "OUTPUT_DIR=%ROOT%publish"
set "RUNTIME=win-x64"
set "DOTNET_CLI_HOME=%ROOT%.dotnet-cli"
set "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1"
set "DOTNET_NOLOGO=1"
set "APPDATA=%ROOT%.appdata"
set "NUGET_PACKAGES=%ROOT%.nuget\packages"

echo.
echo ==========================
echo   Crystrap Share Publish
echo ==========================
echo.

if not exist "%PROJECT%" (
    color 0C
    echo [ERROR] Could not find:
    echo %PROJECT%
    echo.
    pause
    exit /b 1
)

echo [1/3] Checking .NET SDK...
dotnet --version
if errorlevel 1 (
    color 0C
    echo.
    echo [ERROR] dotnet is not available.
    echo Install the .NET 6 SDK, then run this script again.
    echo.
    pause
    exit /b 1
)
echo.

echo [2/3] Restoring packages...
dotnet restore "%PROJECT%"
if errorlevel 1 (
    color 0C
    echo.
    echo [ERROR] Restore failed.
    echo.
    pause
    exit /b 1
)
echo.

echo [3/3] Publishing single-file EXE...
dotnet publish "%PROJECT%" -c Release -r %RUNTIME% --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUTPUT_DIR%"
if errorlevel 1 (
    color 0C
    echo.
    echo [ERROR] Publish failed.
    echo.
    pause
    exit /b 1
)

color 0A
echo.
echo Publish complete.
echo.
echo Share this file:
echo %OUTPUT_DIR%\Crystrap.exe
echo.
echo When the EXE is launched on another Windows PC, Crystrap will still run its normal first-run installer flow.
echo.
pause
exit /b 0
