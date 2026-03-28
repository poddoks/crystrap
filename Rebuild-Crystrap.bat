@echo off
setlocal EnableExtensions
title Crystrap Rebuild
color 0B

set "ROOT=%~dp0"
set "SOLUTION=%ROOT%Crystrap.sln"
set "PROJECT=%ROOT%Bloxstrap\Bloxstrap.csproj"
set "BUILD_DIR=%ROOT%Bloxstrap\bin\Debug\net6.0-windows"
set "INSTALL_DIR=%LOCALAPPDATA%\Crystrap"
set "PUBLISH_DIR=%ROOT%publish"
set "RUNTIME=win-x64"
set "DOTNET_CLI_HOME=%ROOT%.dotnet-cli"
set "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1"
set "DOTNET_NOLOGO=1"
set "APPDATA=%ROOT%.appdata"
set "NUGET_PACKAGES=%ROOT%.nuget\packages"

echo.
echo ==========================
echo   Crystrap Quick Rebuild
echo ==========================
echo.

if not exist "%SOLUTION%" (
    color 0C
    echo [ERROR] Could not find:
    echo %SOLUTION%
    echo.
    pause
    exit /b 1
)

echo [1/4] Checking .NET SDK...
dotnet --version
if errorlevel 1 (
    color 0C
    echo.
    echo [ERROR] dotnet is not available.
    echo Install .NET 6 SDK, then run this script again.
    echo.
    pause
    exit /b 1
)
echo.

echo [2/4] Restoring packages...
dotnet restore "%SOLUTION%"
if errorlevel 1 (
    color 0C
    echo.
    echo [ERROR] Restore failed.
    echo.
    pause
    exit /b 1
)
echo.

echo [3/4] Building Crystrap...
dotnet build "%SOLUTION%" -c Debug --no-restore
if errorlevel 1 (
    color 0C
    echo.
    echo [ERROR] Build failed.
    echo.
    pause
    exit /b 1
)
echo.

echo [4/5] Syncing installed runtime...
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

robocopy "%BUILD_DIR%" "%INSTALL_DIR%" /E /R:1 /W:1 /XD "Backups" "Downloads" "Logs" "Modifications" "Profiles" "Versions" /XF "Settings.json" "State.json" "RobloxState.json" "Data.json" "diagnostic.log" >nul
set "ROBOCODE=%ERRORLEVEL%"
if %ROBOCODE% GEQ 8 (
    color 0E
    echo [WARNING] Build succeeded, but syncing the installed copy hit an issue.
    echo You can still run the build directly from:
    echo %BUILD_DIR%\Crystrap.exe
    echo.
    pause
    exit /b 0
)

echo.
echo [5/5] Publishing shareable single-file EXE...
dotnet publish "%PROJECT%" -c Release -r %RUNTIME% --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%PUBLISH_DIR%"
if errorlevel 1 (
    color 0E
    echo [WARNING] Build and installed copy succeeded, but shareable publish failed.
    echo You can still run the local build from:
    echo %BUILD_DIR%\Crystrap.exe
    echo.
    pause
    exit /b 0
)

color 0A
echo.
echo Build complete.
echo.
echo Built EXE:
echo %BUILD_DIR%\Crystrap.exe
echo.
echo Installed copy updated:
echo %INSTALL_DIR%\Crystrap.exe
echo.
echo Shareable single-file EXE:
echo %PUBLISH_DIR%\Crystrap.exe
echo.
pause
exit /b 0
