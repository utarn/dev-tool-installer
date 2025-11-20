@echo off
setlocal

:: ==================================================================================
:: Project Configuration
:: ==================================================================================
:: Set the project name
set "PROJECT_NAME=DevToolInstaller"
:: Set the runtime identifier (e.g., win-x64, win-arm64)
set "RID=win-x64"
:: Set the output directory for the publish
set "PUBLISH_DIR=%~dp0publish"
:: Set the full path to the output folder for the specific RID
set "OUTPUT_DIR=%PUBLISH_DIR%\%RID%"
:: Set the path to the final executable
set "EXECUTABLE_PATH=%OUTPUT_DIR%\%PROJECT_NAME%.exe"


echo =================================================
echo Building %PROJECT_NAME% for %RID%
echo =================================================

echo Cleaning previous builds in %OUTPUT_DIR%...
if exist "%OUTPUT_DIR%" (
    rmdir /s /q "%OUTPUT_DIR%"
)
mkdir "%OUTPUT_DIR%"

echo.
echo Publishing the application...
dotnet publish -r %RID% -c Release --self-contained true /p:PublishSingleFile=true /p:PublishAot=true /p:PublishTrimmed=true -o "%OUTPUT_DIR%"

if %errorlevel% neq 0 (
    echo.
    echo #################################################
    echo #         Build failed!                         #
    echo #################################################
    exit /b %errorlevel%
)

echo.
echo #################################################
echo #         Build successful!                     #
echo #################################################
echo.

echo #################################################
echo #         Signing the executable...             #
echo #################################################

:: ==================================================================================
:: Signing Configuration
::
:: 1. Ensure signtool.exe is in your system's PATH.
::    (Usually part of the Windows SDK)
::
:: 2. Set CERT_SUBJECT_NAME to the subject name of your code signing certificate.
::    This is found in the certificate details on your USB token.
:: ==================================================================================
set "CERT_SUBJECT_NAME=NEW TECHNOLOGY INFORMATION CO.,"
set "TIMESTAMP_SERVER=http://timestamp.digicert.com"

echo Signing %EXECUTABLE_PATH%...
signtool.exe sign /n "%CERT_SUBJECT_NAME%" /tr "%TIMESTAMP_SERVER%" /td sha256 /fd sha256 /a "%EXECUTABLE_PATH%"

if %errorlevel% neq 0 (
    echo.
    echo #################################################
    echo #         Signing failed!                       #
    echo #################################################
    echo Please ensure your USB token is connected and the certificate subject name is correct.
    exit /b %errorlevel%
)

echo.
echo #################################################
echo #         Signing successful!                   #
echo #################################################
echo.
echo Executable for %RID% created and signed at:
echo %EXECUTABLE_PATH%
echo.

endlocal