#!/bin/bash

# Build script for DevToolInstaller
# Builds for Windows x64 and ARM64 (non-AOT from macOS/Linux)
# For AOT builds, use build.ps1 on Windows

set -e

echo "=========================================="
echo "Building DevToolInstaller for Windows"
echo "=========================================="
echo "NOTE: AOT builds require Windows. This creates portable builds."
echo ""

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf bin obj publish
dotnet clean

# Create publish directory
mkdir -p publish

# Build for Windows x64 (non-AOT)
echo ""
echo "Building for Windows x64 (portable)..."
dotnet publish -c Release -r win-x64 --self-contained -o publish/win-x64 /p:PublishAot=false

# Build for Windows ARM64 (non-AOT)
echo ""
echo "Building for Windows ARM64 (portable)..."
dotnet publish -c Release -r win-arm64 --self-contained -o publish/win-arm64 /p:PublishAot=false

echo ""
echo "=========================================="
echo "Build completed successfully!"
echo "=========================================="
echo "Windows x64 executable: publish/win-x64/DevToolInstaller.exe"
echo "Windows ARM64 executable: publish/win-arm64/DevToolInstaller.exe"
echo ""
echo "NOTE: These are portable (non-AOT) builds."
echo "For native AOT builds with better performance, run build.ps1 on Windows."
echo ""
echo "File sizes:"
ls -lh publish/win-x64/DevToolInstaller.exe 2>/dev/null || echo "x64 build not found"
ls -lh publish/win-arm64/DevToolInstaller.exe 2>/dev/null || echo "ARM64 build not found"