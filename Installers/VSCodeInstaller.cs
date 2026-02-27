using System.Text.Json;
using System.Text.Json.Nodes;

namespace DevToolInstaller.Installers;

public class VSCodeInstaller : IInstaller
{
    private const string DownloadUrl = "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user";
    private const string InstallerFileName = "VSCodeSetup.exe";

    private static readonly string[] Extensions =
    [
        "modelharbor.modelharbor-agent",
        "ms-dotnettools.vscode-dotnet-runtime",
        "formulahendry.dotnet",
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit",
        "ms-dotnettools.vscodeintellicode-csharp",
        "alexcvzz.vscode-sqlite",
        "ms-python.python",
        "PKief.material-icon-theme",
        "shd101wyy.markdown-preview-enhanced",
        "bierner.markdown-mermaid",
        "ms-vscode-remote.remote-ssh",
        "sitoi.ai-commit"
    ];

    /// <summary>
    /// VSCode user settings to apply. These will be merged into existing settings.json
    /// without removing any existing user preferences.
    /// </summary>
    private static readonly Dictionary<string, JsonNode?> UserSettings = new()
    {
        ["workbench.iconTheme"] = JsonValue.Create("material-icon-theme"),
        ["editor.fontFamily"] = JsonValue.Create("'CaskaydiaCove Nerd Font', Consolas, 'Courier New', monospace"),
        ["editor.fontLigatures"] = JsonValue.Create(true),
        ["terminal.integrated.fontFamily"] = JsonValue.Create("CaskaydiaCove Nerd Font"),
        ["terminal.integrated.scrollback"] = JsonValue.Create(10000)
    };

    public string Name => "Visual Studio Code";
    public DevelopmentCategory Category => DevelopmentCategory.CSharp;
    public string Description => "Lightweight but powerful source code editor with extensive extension support";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("code.exe") || ProcessHelper.IsToolInstalled("code");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Visual Studio Code...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading VS Code installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running VS Code installer...");
            progressReporter?.ReportProgress(50);
            var success = ProcessHelper.ExecuteInstaller(installerPath, "/VERYSILENT /NORESTART /MERGETASKS=!runcode");

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(80);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                progressReporter?.ReportStatus("Installing extensions...");
                progressReporter?.ReportProgress(85);
                await InstallExtensionsAsync(progressReporter);

                progressReporter?.ReportStatus("Configuring VS Code settings...");
                progressReporter?.ReportProgress(95);
                ConfigureUserSettings(progressReporter);

                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Visual Studio Code installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Visual Studio Code installation failed");
                return false;
            }
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