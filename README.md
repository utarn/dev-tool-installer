# DevToolInstaller

A high-performance development environment setup tool for Windows that automatically installs and configures essential development tools with AOT (Ahead-of-Time) compilation support.

## Features

- ✅ **AOT Compiled**: Native executables with fast startup and low memory footprint
- 🚀 **Multithreaded Downloads**: Parallel downloads with real-time progress display
- 📊 **Progress Tracking**: Visual feedback with download speed and percentage
- 🎯 **Category-based Selection**: Checkbox UI — select entire categories, batch install
- 🛠️ **27 Tools**: Comprehensive development environment in one click
- 🎨 **31 VS Code Extensions**: Pre-configured for C#, Python, React, Vue, Svelte, and more
- ⚙️ **40+ VS Code Settings**: Pro developer settings applied automatically
- 🔒 **Browser Privacy Settings**: Chrome, Edge, Brave policies via Registry

## Installed Tools (27 total)

### C# Development (1 tool)
| Tool | Description |
|------|-------------|
| .NET 10 SDK | Latest .NET development framework |

### Python Development (5 tools)
| Tool | Description |
|------|-------------|
| Python | Python runtime with PATH configuration |
| pip | Python package manager |
| Poetry | Python dependency management and packaging |
| uv | Ultra-fast Python package installer (Rust-based, replaces pip/virtualenv) |
| Visual C++ Build Tools | Required for compiling native Python packages |

### Node.js Development (4 tools)
| Tool | Description |
|------|-------------|
| NVM for Windows | Node Version Manager for managing multiple Node.js versions |
| Node.js 20 | LTS Node.js runtime (via nvm) |
| npm | Node.js package manager (updated to latest) |
| Node.js Dev Tools | Global npm packages: pnpm, nodemon, express-generator, typescript, ts-node |

### Cross-Platform Tools (17 tools)
| Tool | Description |
|------|-------------|
| Git | Version control system |
| Visual Studio Code | Code editor with 31 extensions and 40+ settings |
| Windows Terminal | Modern terminal application (via winget) |
| PowerShell 7 | Latest PowerShell version |
| Docker Desktop | Container platform with auto-configuration |
| Oh My Posh + Profile | Terminal theme engine with custom Paradox theme, PSReadLine, and Windows Terminal config |
| Developer Fonts | CascadiaMono Nerd Font (downloaded) + TH Sarabun PSK (bundled) |
| Notepad++ | Free source code editor with syntax highlighting |
| Postman | API platform for building, testing, and documenting APIs |
| RustDesk | Open-source remote desktop client |
| Google Chrome | Fast, secure web browser from Google |
| Brave Browser | Privacy-focused Chromium browser with built-in ad blocking |
| Mozilla Firefox | Privacy-focused open source web browser |
| Opera Browser | Feature-rich web browser with built-in VPN and productivity tools |
| Windows Explorer Settings | Show hidden files + show file extensions |
| Browser Privacy Settings | Chrome/Edge/Brave: ask download, disable background/analytics/startup boost, remove startup entries |
| WSL2 Memory Limit | Configure .wslconfig: memory=4GB, swap=8GB, localhostForwarding=true |

## VS Code Extensions (31 auto-installed)

| Category | Extensions |
|----------|-----------|
| **C# / .NET** | modelharbor-agent, vscode-dotnet-runtime, dotnet, csharp, csdevkit, vscodeintellicode-csharp, vscode-sqlite, csharpextensions |
| **Python / Jupyter** | python, debugpy, vscode-pylance, jupyter, ruff |
| **React / Frontend** | es7-react-js-snippets, vscode-tailwindcss, vscode-eslint, prettier, auto-rename-tag, path-intellisense, npm-intellisense, dotenv |
| **Vue.js** | volar |
| **Svelte** | svelte-vscode |
| **General / DevTools** | material-icon-theme, markdown-preview-enhanced, markdown-mermaid, remote-ssh, ai-commit, gitlens, errorlens, vscode-docker |

> If VS Code is already installed, running the installer will **skip download** and only apply extensions + settings.

## VS Code Settings (40+ auto-configured)

Key settings applied (merged into existing `settings.json`):

| Category | Settings |
|----------|----------|
| **Font** | CaskaydiaMono Nerd Font, ligatures, smooth caret |
| **Editor** | Format on save/paste, word wrap, sticky scroll, bracket pair colorization, minimap off |
| **Files** | Auto-save after delay, trim whitespace, insert final newline |
| **Terminal** | Nerd Font, PowerShell default, 10K scrollback |
| **Git** | Auto-fetch, smart commit, no confirm sync |
| **Explorer** | No confirm delete/drag, compact folders off, show .git |
| **Workbench** | No preview tabs, no startup editor, smooth scrolling |

## Browser Privacy Settings (Registry Policies)

Applied to Chrome, Edge, and Brave via `HKCU\SOFTWARE\Policies\`:

| Policy | Effect |
|--------|--------|
| PromptForDownloadLocation | Ask where to save downloads |
| BackgroundModeEnabled | Disable background mode |
| MetricsReportingEnabled | Disable analytics |
| StartupBoostEnabled | Disable startup boost |
| AutofillAddressEnabled | Disable address autofill |
| AutofillCreditCardEnabled | Disable credit card autofill |
| PasswordManagerEnabled | Disable built-in password manager |
| **Startup Entries** | Remove browser auto-launch from Windows Run registry |

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

### Manual Build

```powershell
# Windows x64
dotnet publish DevToolInstaller.csproj -c Release -r win-x64 --self-contained -o publish/win-x64 /p:PublishAot=true /p:StripSymbols=true

# Windows ARM64
dotnet publish DevToolInstaller.csproj -c Release -r win-arm64 --self-contained /p:PublishAot=true /p:StripSymbols=true
```

## Usage

### Running the Installer

1. Copy the entire `publish/win-x64/` folder to the target machine
2. **Double-click to run** — the app auto-elevates to Administrator
3. Select categories with **Space** to toggle, **A** to select all
4. Press **Enter** to batch install all tools in selected categories
5. The installer will download, install, and configure everything
6. After completion, choose to **restart your computer** when prompted

### Controls

| Key | Action |
|-----|--------|
| ↑↓ | Navigate categories |
| Space | Toggle category selection |
| A | Select / deselect all |
| Enter | Install all selected |
| Esc | Exit |

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
- **MenuSystem**: Interactive TUI with category-level checkboxes and batch install
- **IInstaller Interface**: Common interface for all tool installers
- **ToolRegistry**: Central registry of all available installers

### Categories

| Category | Tools |
|----------|-------|
| C# Development | .NET 10 SDK |
| Python Development | Python, pip, Poetry, uv, VC++ Build Tools |
| Node.js Development | NVM, Node.js 20, npm, Dev Tools (pnpm, nodemon, typescript, ts-node) |
| Cross-Platform Tools | Git, VS Code, Terminal, PowerShell 7, Docker, Oh My Posh, Fonts, Notepad++, Postman, RustDesk, Chrome, Brave, Firefox, Opera, Explorer Settings, Browser Settings, WSL2 Config |

### AOT Compatibility

- Uses JSON source generation (`System.Text.Json.Nodes`)
- No reflection-based serialization
- Native code generation for optimal performance

### Design Patterns

- **Strategy Pattern**: Each installer implements `IInstaller`
- **Async/Await**: Non-blocking I/O operations
- **Thread-Safe**: Console output synchronization
- **Merge-based Config**: VSCode/Terminal settings are merged, not overwritten
- **Registry Policies**: Browser settings via HKCU policies (no admin required)

## System Requirements

### Target Systems
- Windows 10/11 x64 or ARM64
- Administrator privileges (required for font installation, recommended for all)
- Internet connection for downloads

### Development System
- .NET 10 SDK
- Windows (for AOT builds)

## Post-Installation

After installation:
1. **Restart your terminal** to refresh environment variables
2. **Restart browsers** to apply privacy settings
3. **Restart your computer** (optional, recommended)
4. Verify:
   ```powershell
   dotnet --version
   code --version
   git --version
   pwsh --version
   docker --version
   python --version
   uv --version
   node --version
   pnpm --version
   ```

## Troubleshooting

### Common Issues

**"Not running as Administrator"**
- The app auto-elevates to Administrator on startup
- If UAC prompt appears, click "Yes" to continue

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

**Browser settings not applying**
- Restart the browser completely
- Check `chrome://policy` (Chrome) or `edge://policy` (Edge) to verify

## License

This project is provided as-is for development environment setup purposes.