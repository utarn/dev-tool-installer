# DevToolInstaller

A high-performance development environment setup tool for Windows that automatically installs and configures essential development tools with AOT (Ahead-of-Time) compilation support.

## Features

- ✅ **AOT Compiled**: Native executables with fast startup and low memory footprint
- 🚀 **Multithreaded Downloads**: Parallel downloads with real-time progress display
- 📊 **Progress Tracking**: Visual feedback with download speed and percentage
- 🎯 **Multiple Platforms**: Supports Windows x64 and ARM64
- 🛠️ **Comprehensive Tool Suite**: Installs all essential development tools
- 🎨 **TUI Menu**: Interactive category-based menu for selecting tools

## Installed Tools

### C# Development
| Tool | Description |
|------|-------------|
| .NET 10 SDK | Latest .NET development framework |
| Visual Studio Code | Code editor with extensions and settings configuration |

### Python Development
| Tool | Description |
|------|-------------|
| Python | Python runtime with PATH configuration |
| pip | Python package manager |
| Poetry | Python dependency management and packaging |
| Visual C++ Build Tools | Required for compiling native Python packages |

### Node.js Development
| Tool | Description |
|------|-------------|
| NVM for Windows | Node Version Manager for managing multiple Node.js versions |
| Node.js 20 | LTS Node.js runtime (via nvm) |
| npm | Node.js package manager |
| Node.js Tools | Global npm packages (yarn, pnpm, etc.) |
| Flowise | Low-code AI workflow builder |

### Cross-Platform Tools
| Tool | Description |
|------|-------------|
| Git | Version control system |
| Windows Terminal | Modern terminal application (via winget) |
| PowerShell 7 | Latest PowerShell version |
| Docker Desktop | Container platform with auto-configuration |
| PostgreSQL | Relational database (via winget) |
| Oh My Posh + Profile | Terminal theme engine with custom Paradox theme, PSReadLine, and Windows Terminal config |
| Developer Fonts | CascadiaMono Nerd Font (downloaded) + TH Sarabun PSK (bundled) |
| Postman | API platform for building, testing, and documenting APIs |
| RustDesk | Open-source remote desktop client |

### VS Code Extensions (auto-installed)

| Extension | Description |
|-----------|-------------|
| `modelharbor.modelharbor-agent` | ModelHarbor Agent |
| `ms-dotnettools.vscode-dotnet-runtime` | .NET Runtime |
| `formulahendry.dotnet` | .NET Tools |
| `ms-dotnettools.csharp` | C# language support |
| `ms-dotnettools.csdevkit` | C# Dev Kit |
| `ms-dotnettools.vscodeintellicode-csharp` | IntelliCode for C# |
| `alexcvzz.vscode-sqlite` | SQLite Viewer |
| `ms-python.python` | Python |
| `PKief.material-icon-theme` | Material Icon Theme |
| `shd101wyy.markdown-preview-enhanced` | Markdown Preview Enhanced |
| `bierner.markdown-mermaid` | Markdown Mermaid diagrams |
| `ms-vscode-remote.remote-ssh` | Remote SSH |
| `sitoi.ai-commit` | AI Commit message generator |

### VS Code Settings (auto-configured)

| Setting | Value |
|---------|-------|
| `workbench.iconTheme` | `material-icon-theme` |
| `editor.fontFamily` | `CaskaydiaCove Nerd Font` |
| `editor.fontLigatures` | `true` |
| `terminal.integrated.fontFamily` | `CaskaydiaCove Nerd Font` |
| `terminal.integrated.scrollback` | `10000` |

Settings are merged into `%APPDATA%\Code\User\settings.json` without removing existing user preferences.

## Building

### Prerequisites

- .NET 10 SDK
- **For AOT builds:** Must build on Windows (x64 or ARM64)

Install .NET 10 SDK:
```powershell
winget install Microsoft.DotNet.SDK.10
```

### Build Script

```powershell
.\build.ps1
```

This creates native executables:
- `publish/win-x64/DevToolInstaller.exe` - For Windows x64
- `publish/win-arm64/DevToolInstaller.exe` - For Windows ARM64

### Manual Build

```powershell
# Windows x64
dotnet publish DevToolInstaller.csproj -c Release -r win-x64 --self-contained /p:PublishAot=true /p:StripSymbols=true

# Windows ARM64
dotnet publish DevToolInstaller.csproj -c Release -r win-arm64 --self-contained /p:PublishAot=true /p:StripSymbols=true
```

## Usage

### Running the Installer

1. Copy the entire `publish/win-x64/` folder to the target machine (includes exe + bundled files)
2. **Run as Administrator** (right-click → "Run as Administrator")
3. Select tools from the interactive category menu
4. The installer will download, install, and configure everything

### Bundled Files

The exe requires these files alongside it:
- `config/paradox.omp.json` - Oh My Posh custom theme
- `font/THSARABUN_PSK.zip` - TH Sarabun PSK fonts

> **Note:** CascadiaMono Nerd Font is downloaded at runtime from GitHub releases (not bundled due to size).

## Architecture

### Key Components

- **DownloadManager**: Handles multithreaded downloads with progress tracking
- **ConsoleHelper**: Thread-safe console output with colored formatting
- **ProcessHelper**: Manages process execution and tool detection
- **MenuSystem**: Interactive TUI with category-based navigation
- **IInstaller Interface**: Common interface for all tool installers
- **ToolRegistry**: Central registry of all available installers

### Categories

| Category | Description |
|----------|-------------|
| C# Development | .NET SDK, VS Code |
| Python Development | Python, pip, Poetry, VC++ Build Tools |
| Node.js Development | NVM, Node.js, npm, tools, Flowise |
| Cross-Platform Tools | Git, Terminal, Docker, PostgreSQL, fonts, etc. |

### AOT Compatibility

- Uses JSON source generation (`System.Text.Json.Nodes`)
- No reflection-based serialization
- Native code generation for optimal performance

### Design Patterns

- **Strategy Pattern**: Each installer implements `IInstaller`
- **Async/Await**: Non-blocking I/O operations
- **Thread-Safe**: Console output synchronization
- **Merge-based Config**: VSCode/Terminal settings are merged, not overwritten

## System Requirements

### Target Systems
- Windows 11 x64 or ARM64
- Administrator privileges (required for font installation, recommended for all)
- Internet connection for downloads

### Development System
- .NET 10 SDK
- Windows (for AOT builds)

## Post-Installation

After installation:
1. **Restart your terminal** to refresh environment variables
2. **Restart your computer** (optional, recommended)
3. Verify:
   ```powershell
   dotnet --version
   code --version
   git --version
   pwsh --version
   docker --version
   python --version
   node --version
   ```

## Troubleshooting

### Common Issues

**"Not running as Administrator"**
- Right-click the executable → "Run as Administrator"

**"Tool already installed" but not detected**
- Close and reopen your terminal
- Check PATH environment variable
- Restart your computer

**Download failures**
- Check internet connection
- Verify firewall settings
- Try running again

**Docker Desktop issues**
- Wait for Docker to fully start
- Check WSL2 is enabled
- Verify virtualization is enabled in BIOS

## License

This project is provided as-is for development environment setup purposes.