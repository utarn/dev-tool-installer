using System.Text.Json;
using System.Text.Json.Nodes;

namespace DevToolInstaller.Installers;

public class VSCodeInstaller : IInstaller
{
    private const string DownloadUrl = "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user";
    private const string InstallerFileName = "VSCodeSetup.exe";

    private static readonly string[] Extensions =
    [
        // C# / .NET
        "modelharbor.modelharbor-agent",
        "ms-dotnettools.vscode-dotnet-runtime",
        "formulahendry.dotnet",
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit",
        "ms-dotnettools.vscodeintellicode-csharp",
        "alexcvzz.vscode-sqlite",
        "kreativ-software.csharpextensions",

        // Python / Jupyter
        "ms-python.python",
        "ms-python.debugpy",
        "ms-python.vscode-pylance",
        "ms-toolsai.jupyter",
        "charliermarsh.ruff",

        // React / Next.js / Frontend
        "dsznajder.es7-react-js-snippets",
        "bradlc.vscode-tailwindcss",
        "dbaeumer.vscode-eslint",
        "esbenp.prettier-vscode",
        "formulahendry.auto-rename-tag",
        "christian-kohler.path-intellisense",
        "christian-kohler.npm-intellisense",
        "mikestead.dotenv",

        // Vue.js
        "Vue.volar",

        // Svelte
        "svelte.svelte-vscode",

        // General / Markdown / DevTools
        "PKief.material-icon-theme",
        "shd101wyy.markdown-preview-enhanced",
        "bierner.markdown-mermaid",
        "ms-vscode-remote.remote-ssh",
        "sitoi.ai-commit",
        "eamodio.gitlens",
        "usernamehw.errorlens",
        "ms-azuretools.vscode-docker"
    ];

    /// <summary>
    /// VSCode user settings to apply. These will be merged into existing settings.json
    /// without removing any existing user preferences.
    /// </summary>
    private static readonly Dictionary<string, JsonNode?> UserSettings = new()
    {
        // Appearance
        ["workbench.iconTheme"] = JsonValue.Create("material-icon-theme"),

        // Font
        ["editor.fontFamily"] = JsonValue.Create("'CaskaydiaMono Nerd Font', Consolas, 'Courier New', monospace"),
        ["editor.fontSize"] = JsonValue.Create(14),
        ["editor.fontLigatures"] = JsonValue.Create(true),
        ["editor.cursorSmoothCaretAnimation"] = JsonValue.Create("on"),
        ["editor.cursorBlinking"] = JsonValue.Create("smooth"),

        // Editor behavior
        ["editor.formatOnSave"] = JsonValue.Create(true),
        ["editor.formatOnPaste"] = JsonValue.Create(true),
        ["editor.linkedEditing"] = JsonValue.Create(true),
        ["editor.wordWrap"] = JsonValue.Create("on"),
        ["editor.stickyScroll.enabled"] = JsonValue.Create(true),
        ["editor.guides.bracketPairs"] = JsonValue.Create(true),
        ["editor.bracketPairColorization.enabled"] = JsonValue.Create(true),
        ["editor.minimap.enabled"] = JsonValue.Create(false),
        ["editor.renderWhitespace"] = JsonValue.Create("boundary"),
        ["editor.suggestSelection"] = JsonValue.Create("first"),
        ["editor.acceptSuggestionOnCommitCharacter"] = JsonValue.Create(false),
        ["editor.inlineSuggest.enabled"] = JsonValue.Create(true),
        ["editor.tabSize"] = JsonValue.Create(2),
        ["editor.detectIndentation"] = JsonValue.Create(true),
        ["editor.smoothScrolling"] = JsonValue.Create(true),

        // File handling
        ["files.autoSave"] = JsonValue.Create("afterDelay"),
        ["files.autoSaveDelay"] = JsonValue.Create(1000),
        ["files.trimTrailingWhitespace"] = JsonValue.Create(true),
        ["files.insertFinalNewline"] = JsonValue.Create(true),
        ["files.trimFinalNewlines"] = JsonValue.Create(true),

        // Explorer
        ["explorer.confirmDelete"] = JsonValue.Create(false),
        ["explorer.confirmDragAndDrop"] = JsonValue.Create(false),
        ["explorer.compactFolders"] = JsonValue.Create(false),

        // Terminal
        ["terminal.integrated.fontFamily"] = JsonValue.Create("CaskaydiaMono Nerd Font"),
        ["terminal.integrated.fontSize"] = JsonValue.Create(13),
        ["terminal.integrated.scrollback"] = JsonValue.Create(10000),
        ["terminal.integrated.defaultProfile.windows"] = JsonValue.Create("PowerShell"),
        ["terminal.integrated.smoothScrolling"] = JsonValue.Create(true),

        // Git
        ["git.autofetch"] = JsonValue.Create(true),
        ["git.confirmSync"] = JsonValue.Create(false),
        ["git.enableSmartCommit"] = JsonValue.Create(true),

        // Workbench
        ["workbench.editor.enablePreview"] = JsonValue.Create(false),
        ["workbench.startupEditor"] = JsonValue.Create("none"),
        ["workbench.list.smoothScrolling"] = JsonValue.Create(true),
        ["workbench.tree.indent"] = JsonValue.Create(16),

        // Breadcrumbs & search
        ["breadcrumbs.enabled"] = JsonValue.Create(true),
        ["search.smartCase"] = JsonValue.Create(true)
    };

    public string Name => "Visual Studio Code";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Lightweight but powerful source code editor with extensive extension support";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("code.exe") || ProcessHelper.IsToolInstalled("code");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Visual Studio Code...");

        try
        {
            // Check if VSCode is already installed — skip download/install, just do extensions + settings
            var alreadyInstalled = await ProcessHelper.FindExecutableInPathAsync("code.exe") || ProcessHelper.IsToolInstalled("code");

            if (alreadyInstalled)
            {
                progressReporter?.ReportStatus("VS Code already installed. Configuring extensions & settings...");
                progressReporter?.ReportProgress(10);
            }
            else
            {
                var tempPath = Path.GetTempPath();
                var installerPath = Path.Combine(tempPath, InstallerFileName);

                progressReporter?.ReportStatus("Downloading VS Code installer...");
                progressReporter?.ReportProgress(10);
                await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

                progressReporter?.ReportStatus("Running VS Code installer...");
                progressReporter?.ReportProgress(50);
                var success = ProcessHelper.ExecuteInstaller(installerPath, "/VERYSILENT /NORESTART /MERGETASKS=!runcode");

                progressReporter?.ReportStatus("Cleaning up...");
                progressReporter?.ReportProgress(70);
                if (File.Exists(installerPath))
                {
                    File.Delete(installerPath);
                }

                if (!success)
                {
                    progressReporter?.ReportError("Visual Studio Code installation failed");
                    return false;
                }
            }

            // Always install extensions and configure settings (even if VSCode was already installed)
            progressReporter?.ReportStatus("Installing extensions...");
            progressReporter?.ReportProgress(75);
            await InstallExtensionsAsync(progressReporter);

            progressReporter?.ReportStatus("Configuring VS Code settings...");
            progressReporter?.ReportProgress(95);
            ConfigureUserSettings(progressReporter);

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess(
                alreadyInstalled
                    ? "VS Code extensions & settings configured successfully!"
                    : "Visual Studio Code installation completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Visual Studio Code: {ex.Message}");
            return false;
        }
    }

    private async Task InstallExtensionsAsync(IProgressReporter? progressReporter = null)
    {
        progressReporter?.ReportStatus("Installing VS Code extensions...");
        
        var totalExtensions = Extensions.Length;
        for (int i = 0; i < totalExtensions; i++)
        {
            var extension = Extensions[i];
            try
            {
                progressReporter?.ReportStatus($"Installing extension: {extension}");
                var progress = 90 + (i * 10 / totalExtensions);
                progressReporter?.ReportProgress(progress);
                await ProcessHelper.GetCommandOutput("code", $"--install-extension {extension}");
                await Task.Delay(1000); // Brief delay between installations
            }
            catch (Exception ex)
            {
                progressReporter?.ReportWarning($"Failed to install extension {extension}: {ex.Message}");
            }
        }
        
        progressReporter?.ReportSuccess("VS Code extensions installation completed!");
    }

    /// <summary>
    /// Merges <see cref="UserSettings"/> into the VS Code user settings.json.
    /// Preserves all existing settings and only adds/overwrites the keys defined above.
    /// Path: %APPDATA%\Code\User\settings.json
    /// </summary>
    private static void ConfigureUserSettings(IProgressReporter? progressReporter)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(appData))
            {
                progressReporter?.ReportWarning("Could not determine APPDATA path. Skipping VS Code settings.");
                return;
            }

            var settingsDir = Path.Combine(appData, "Code", "User");
            var settingsPath = Path.Combine(settingsDir, "settings.json");

            JsonObject root;
            if (File.Exists(settingsPath))
            {
                var jsonText = File.ReadAllText(settingsPath);
                var parsed = JsonNode.Parse(jsonText, documentOptions: new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });
                root = parsed as JsonObject ?? new JsonObject();
            }
            else
            {
                if (!Directory.Exists(settingsDir))
                    Directory.CreateDirectory(settingsDir);
                root = new JsonObject();
            }

            // Merge settings – only set keys that are defined; leave everything else untouched
            foreach (var (key, value) in UserSettings)
            {
                root[key] = value?.DeepClone();
            }

            // Merge files.exclude – show .git objects by default
            if (root["files.exclude"] is JsonObject existingExclude)
            {
                existingExclude["**/.git"] = false;
            }
            else
            {
                root["files.exclude"] = new JsonObject
                {
                    ["**/.git"] = false
                };
            }

            var writeOptions = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = root.ToJsonString(writeOptions);
            File.WriteAllText(settingsPath, updatedJson);

            progressReporter?.ReportStatus($"VS Code settings configured: {settingsPath}");
        }
        catch (Exception ex)
        {
            progressReporter?.ReportWarning($"Could not configure VS Code settings: {ex.Message}");
        }
    }
}