# Node.js Development Tool Installers

This document describes the Node.js development tool installers implemented in the DevToolInstaller application.

## Overview

The Node.js development tools include three main installers:

1. **NodeJsInstaller** - Installs Node.js runtime with npm
2. **NpmInstaller** - Ensures npm is up to date
3. **NodeJsToolsInstaller** - Installs common Node.js development tools

## NodeJsInstaller

### Purpose
Installs the Node.js JavaScript runtime environment with npm included.

### Details
- **Download URL**: Uses Node.js LTS version (v20.12.2) from nodejs.org
- **Installation Method**: MSI installer with quiet installation
- **Category**: NodeJS
- **Dependencies**: None
- **Features**:
  - Downloads Node.js LTS version
  - Installs with npm included
  - Verifies installation using `node` command
  - Cleans up installer files after installation

### Installation Process
1. Downloads Node.js MSI installer
2. Runs installer with `/quiet /norestart ADDLOCAL=ALL` parameters
3. Verifies installation by checking if `node` command is available
4. Cleans up temporary files

## NpmInstaller

### Purpose
Ensures npm (Node Package Manager) is installed and up to date.

### Details
- **Installation Method**: Uses npm to update itself
- **Category**: NodeJS
- **Dependencies**: Node.js (NodeJsInstaller)
- **Features**:
  - Checks if npm is already installed
  - Updates npm to the latest version using `npm install -g npm@latest`
  - Verifies installation using `npm --version` command

### Installation Process
1. Checks if npm is already installed
2. Runs `npm install -g npm@latest` to update to latest version
3. Verifies successful installation

## NodeJsToolsInstaller

### Purpose
Installs common Node.js development tools for enhanced development experience.

### Details
- **Installation Method**: Uses npm to install global packages
- **Category**: NodeJS
- **Dependencies**: Node.js (NodeJsInstaller)
- **Tools Installed**:
  - **nodemon**: Automatically restarts Node.js applications on file changes
  - **express-generator**: Scaffolds Express.js applications
  - **typescript**: TypeScript compiler for JavaScript development
  - **ts-node**: Executes TypeScript files directly in Node.js

### Installation Process
1. Checks each tool to see if it's already installed using `npm list -g <package-name>`
2. Installs missing tools using `npm install -g <package-name>`
3. Provides detailed progress feedback for each tool
4. Reports overall installation status

### Features
- **Selective Installation**: Only installs tools that aren't already present
- **Progress Tracking**: Shows installation progress for each tool
- **Error Handling**: Continues installation even if individual tools fail
- **Detailed Feedback**: Provides clear status messages for each step

## Usage in Application

These installers are registered in the `ToolRegistry` and can be accessed through:
- Main menu under "Node.js Development" category
- Individual tool selection
- Dependency-aware installation (Node.js is installed before npm and tools)

## Error Handling

All installers include comprehensive error handling:
- Network download failures
- Installation process failures
- Permission issues
- Dependency conflicts
- Graceful cleanup of temporary files

## Verification

Each installer includes verification logic:
- Checks if tools are already installed before attempting installation
- Uses appropriate commands to verify successful installation
- Provides clear feedback about installation status
- Handles partial installations gracefully