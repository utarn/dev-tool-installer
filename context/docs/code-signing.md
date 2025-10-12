# Code Signing Feature

## Overview
The build script for DevToolInstaller includes code signing functionality for the Windows x64 executable. This ensures the executable is digitally signed using a certificate from a USB token, providing authenticity and security for users.

## Implementation Details
- **Tool**: Uses `signtool.exe` from the Windows SDK
- **Certificate Selection**: Automatically selects the best certificate from the USB token using `/a` parameter
- **Hash Algorithm**: SHA256 for both file hashing and timestamping
- **Timestamp Server**: Uses DigiCert's timestamp server

## Process Flow
1. After building the x64 executable, the build script attempts to sign it
2. If signing fails (e.g., USB token not connected), the build continues with a warning
3. The script provides clear feedback about the signing status

## Requirements
- Windows SDK installed (provides signtool.exe)
- USB token with a valid code signing certificate connected to the computer
- Appropriate permissions to access the certificate on the USB token

## Build Script Integration
The code signing is integrated into the `build.ps1` script and specifically targets the x64 executable. The ARM64 executable is not signed as it's typically used on different devices where the USB token might not be available.

## Troubleshooting
- Ensure the USB token is properly connected and recognized by Windows
- Verify that signtool.exe is in your system PATH
- Check that you have the necessary permissions to access the certificate