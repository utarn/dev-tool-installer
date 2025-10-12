# UAC Elevation Feature

## Overview
The DevToolInstaller now includes functionality to automatically request administrative privileges through the Windows User Account Control (UAC) system.

## How It Works
1. When the application starts, it checks if it's running with administrator privileges
2. If not running as administrator, the application automatically requests elevation through the Windows UAC prompt
3. The user can either:
   - Approve the UAC prompt to continue with elevated privileges
   - Deny the UAC prompt (which will terminate the application)

## Implementation Details
- **ProcessHelper.cs**: Added `RestartAsAdministrator()` method that uses the Windows "runas" verb to restart the application with elevated privileges
- **Program.cs**: Modified the privilege check to provide the user with the option to restart with elevated privileges
- **app.manifest**: Added an application manifest file to ensure proper Windows compatibility and UAC behavior
- **DevToolInstaller.csproj**: Updated to include the application manifest in the build

## Benefits
- Improved user experience by providing a one-click option to elevate privileges
- Better installation success rate when running with appropriate permissions
- Maintains backward compatibility by allowing users to continue without elevation if they choose

## Notes
- This functionality is only available on Windows systems
- On non-Windows systems, the application will continue to run without elevation attempts
- The application will exit and restart when elevation is requested