# DevToolInstaller

A high-performance C# development environment setup tool for Windows that automatically installs and configures essential development tools with AOT (Ahead-of-Time) compilation support.

## Features

- ✅ **AOT Compiled**: Native executables with fast startup and low memory footprint
- 🚀 **Multithreaded Downloads**: Parallel downloads with real-time progress display
- 📊 **Progress Tracking**: Visual feedback with download speed and percentage
- 🎯 **Multiple Platforms**: Supports Windows x64 and ARM64
- 🛠️ **Comprehensive Tool Suite**: Installs all essential development tools

## Installed Tools

The installer automatically sets up:

1. **.NET 8 SDK** - Latest .NET development framework
2. **Visual Studio Code** - Code editor with essential extensions
   - ModelHarbor Agent
   - .NET Runtime
   - .NET Tools
   - C# DevKit
   - IntelliCode for C#
   - SQLite Viewer
3. **Git** - Version control system
4. **Windows Terminal** - Modern terminal application
5. **PowerShell 7** - Latest PowerShell version
6. **Docker Desktop** - Container platform
   - Automatically configured with optimal settings
   - Pulls pgvector/pgvector:pg17 image
   - Configured to start on boot
7. **Ngrok** - Secure tunneling service

## Building

### Prerequisites

**IMPORTANT:** AOT (Native) compilation must be done on the target platform. You cannot cross-compile from macOS to Windows with AOT enabled.

- .NET 9 SDK or later
- **For AOT builds:** Must build on Windows (x64 or ARM64)
- For non-AOT builds: Any platform with .NET 9 SDK

### Build Scripts

**On Windows (for AOT builds):**
```powershell
.\build.ps1
```

The build script will create two native executables:
- `publish/win-x64/DevToolInstaller.exe` - For Windows x64
- `publish/win-arm64/DevToolInstaller.exe` - For Windows ARM64

### Manual Build

**On Windows for AOT compilation:**

Build for Windows x64:
```bash
dotnet publish -c Release -r win-x64 --self-contained -o publish/win-x64 /p:PublishAot=true
```

Build for Windows ARM64:
```bash
dotnet publish -c Release -r win-arm64 --self-contained -o publish/win-arm64 /p:PublishAot=true
```

**On macOS/Linux (non-AOT, portable):**

You can build a portable (non-AOT) version that will work on Windows:
```bash
dotnet publish -c Release -r win-x64 --self-contained -o publish/win-x64
dotnet publish -c Release -r win-arm64 --self-contained -o publish/win-arm64
```

Note: These will be larger and slower than AOT builds but will work cross-platform.

## Usage

### Running the Installer

1. **Download** the appropriate executable for your system architecture
2. **Run as Administrator** (right-click → "Run as Administrator")
3. The installer will:
   - Check for administrator privileges
   - Detect already installed tools
   - Download and install missing tools
   - Display progress for each installation
   - Configure tools with optimal settings

### Command Line Options

The installer runs interactively and will:
- Prompt for confirmation if not running as Administrator
- Skip tools that are already installed
- Display real-time download progress
- Show a summary of installations upon completion

### Installation Flow

```
DevToolInstaller.exe
├── Administrator Check
├── .NET 8 SDK
│   ├── Check if installed
│   ├── Download installer
│   ├── Install silently
│   └── Verify installation
├── Visual Studio Code
│   ├── Check if installed
│   ├── Download installer
│   ├── Install silently
│   └── Install extensions
├── Git
│   └── ...
├── Windows Terminal
│   └── (via winget)
├── PowerShell 7
│   └── ...
├── Docker Desktop
│   ├── Install
│   ├── Configure settings
│   ├── Enable auto-start
│   ├── Pull pgvector image
│   └── Start service
└── Ngrok
    └── ...
```

## Architecture

### Key Components

- **DownloadManager**: Handles multithreaded downloads with progress tracking
- **ConsoleHelper**: Thread-safe console output with colored formatting
- **ProcessHelper**: Manages process execution and tool detection
- **IInstaller Interface**: Common interface for all tool installers
- **Individual Installers**: Specialized installers for each tool

### AOT Compatibility

The application is fully AOT-compatible:
- Uses JSON source generation for Docker settings
- No reflection-based serialization
- All async methods properly structured
- Native code generation for optimal performance

### Design Patterns

- **Strategy Pattern**: Each installer implements `IInstaller`
- **Factory Pattern**: Dynamic installer instantiation
- **Async/Await**: Non-blocking I/O operations
- **Thread-Safe**: Console output synchronization

## Technical Details

### Download System

- Asynchronous HTTP downloads
- 8KB buffer size for optimal performance
- Real-time progress calculation
- Download speed monitoring (MB/s)
- Automatic retry and error handling

### Progress Display

```
Progress: 45% (23.5 MB / 52.3 MB) - Speed: 8.32 MB/s
```

### Tool Detection

- Checks PATH environment variable
- Validates tool availability via command execution
- Detects existing installations to avoid duplicates

## System Requirements

### Target Systems
- Windows 11 x64 or ARM64
- Administrator privileges (recommended)
- Internet connection for downloads

### Development System
- .NET 9 SDK
- macOS M1/M2/M3 (for building)
- Windows (for building)

## Error Handling

The installer includes comprehensive error handling:
- Network timeout handling (30-minute timeout)
- File access error management
- Process execution error catching
- Graceful degradation for non-critical failures
- Detailed error messages with troubleshooting info

## Post-Installation

After installation completes:
1. **Restart your terminal** to refresh environment variables
2. **Restart your computer** (optional, recommended for full effect)
3. Verify installations:
   ```bash
   dotnet --version
   code --version
   git --version
   pwsh --version
   docker --version
   ngrok version
   ```

## Contributing

This project uses:
- .NET 9 with AOT compilation
- C# 12 language features
- Modern async/await patterns
- JSON source generation

## License

This project is provided as-is for development environment setup purposes.

## Troubleshooting

### Common Issues

**"Not running as Administrator"**
- Right-click the executable
- Select "Run as Administrator"

**"Tool already installed" but not detected**
- Close and reopen your terminal
- Check PATH environment variable
- Restart your computer

**Download failures**
- Check internet connection
- Verify firewall settings
- Try running again (automatic retry)

**Docker Desktop issues**
- Wait for Docker to fully start
- Check Windows Subsystem for Linux (WSL2) is enabled
- Verify virtualization is enabled in BIOS

## Version History

- **v1.0.0**: Initial release with AOT support and multithreaded downloads

## Support

For issues or questions, please check:
1. Windows Event Viewer for installation errors
2. Tool-specific documentation
3. Ensure all prerequisites are met