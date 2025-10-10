# Build script for DevToolInstaller
# Builds for Windows x64 and ARM64 with AOT support

Write-Host "==========================================" -ForegroundColor Magenta
Write-Host "Building DevToolInstaller for Windows" -ForegroundColor Magenta
Write-Host "==========================================" -ForegroundColor Magenta

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Cyan
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
if (Test-Path "publish") { Remove-Item -Recurse -Force "publish" }
dotnet clean

# Create publish directory
New-Item -ItemType Directory -Force -Path "publish" | Out-Null

# Build for Windows x64
Write-Host ""
Write-Host "Building for Windows x64..." -ForegroundColor Green
dotnet publish -c Release -r win-x64 --self-contained -o publish/win-x64 /p:PublishAot=true /p:StripSymbols=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed for Windows x64" -ForegroundColor Red
    exit 1
}

# Build for Windows ARM64
Write-Host ""
Write-Host "Building for Windows ARM64..." -ForegroundColor Green
dotnet publish -c Release -r win-arm64 --self-contained -o publish/win-arm64 /p:PublishAot=true /p:StripSymbols=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed for Windows ARM64" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Magenta
Write-Host "Build completed successfully!" -ForegroundColor Magenta
Write-Host "==========================================" -ForegroundColor Magenta
Write-Host "Windows x64 executable: publish/win-x64/DevToolInstaller.exe" -ForegroundColor Cyan
Write-Host "Windows ARM64 executable: publish/win-arm64/DevToolInstaller.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "File sizes:" -ForegroundColor Yellow
Get-ChildItem "publish/win-x64/DevToolInstaller.exe" | Format-Table Name, @{Name="Size (MB)"; Expression={[math]::Round($_.Length / 1MB, 2)}}
Get-ChildItem "publish/win-arm64/DevToolInstaller.exe" | Format-Table Name, @{Name="Size (MB)"; Expression={[math]::Round($_.Length / 1MB, 2)}}