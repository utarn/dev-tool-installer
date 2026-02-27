using System.Text.Json;
using System.Text.Json.Nodes;

namespace DevToolInstaller.Installers;

public class OhMyPoshInstaller : IInstaller
{
    private static readonly string BundledThemePath =
        Path.Combine(AppContext.BaseDirectory, "config", "paradox.omp.json");

    public string Name => "Oh My Posh + Profile";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    // PowerShell Core GUID used by Windows Terminal
    private const string PowerShellCoreGuid = "{574e775e-4f2a-5b96-ac1e-a2962a402336}";

    public string Description => "Terminal prompt theme engine with custom Paradox theme, PSReadLine history, Python venv display & Windows Terminal config";
    public List<string> Dependencies => new() { "PowerShell 7" };

    public async Task<bool> IsInstalledAsync()
    {
        // Check if oh-my-posh executable exists
        if (await ProcessHelper.FindExecutableInPathAsync("oh-my-posh") || ProcessHelper.IsToolInstalled("oh-my-posh"))
        {
            // Also check if the profile is already configured
            var profilePath = GetPwshProfilePath();
            if (profilePath != null && File.Exists(profilePath))
            {
                var content = File.ReadAllText(profilePath);
                if (content.Contains("oh-my-posh init pwsh"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Oh My Posh + PowerShell Profile...");

        try
        {
            // Step 1: Install oh-my-posh via winget
            var ompInstalled = await ProcessHelper.FindExecutableInPathAsync("oh-my-posh") || ProcessHelper.IsToolInstalled("oh-my-posh");

            if (!ompInstalled)
            {
                if (ProcessHelper.IsToolInstalled("winget"))
                {
                    progressReporter?.ReportStatus("Installing Oh My Posh via winget...");
                    progressReporter?.ReportProgress(10);
                    var output = await ProcessHelper.GetCommandOutput("winget",
                        "install --id=JanDeDobbeleer.OhMyPosh -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                    if (output == null)
                    {
                        progressReporter?.ReportError("Failed to install Oh My Posh via winget");
                        return false;
                    }

                    // Refresh PATH so oh-my-posh is findable
                    ProcessHelper.RefreshEnvironmentVariables();
                }
                else
                {
                    progressReporter?.ReportError("winget not found. Cannot install Oh My Posh.");
                    return false;
                }
            }
            else
            {
                progressReporter?.ReportStatus("Oh My Posh is already installed, skipping binary installation...");
            }

            progressReporter?.ReportProgress(40);

            // Step 2: Determine PowerShell profile directory and copy theme
            var profilePath = GetPwshProfilePath();
            if (profilePath == null)
            {
                progressReporter?.ReportError("Could not determine PowerShell 7 profile path. Is PowerShell 7 installed?");
                return false;
            }

            var profileDir = Path.GetDirectoryName(profilePath)!;
            if (!Directory.Exists(profileDir))
            {
                Directory.CreateDirectory(profileDir);
            }

            // Copy bundled theme to profile directory
            var targetThemePath = Path.Combine(profileDir, "paradox.omp.json");
            progressReporter?.ReportStatus("Copying custom Paradox theme...");
            progressReporter?.ReportProgress(55);

            if (File.Exists(BundledThemePath))
            {
                File.Copy(BundledThemePath, targetThemePath, overwrite: true);
            }
            else
            {
                progressReporter?.ReportWarning($"Bundled theme not found at: {BundledThemePath}. Will use built-in paradox theme.");
                // If bundled file not found, we'll reference the built-in theme name instead
            }

            // Step 3: Create/update PowerShell profile
            progressReporter?.ReportStatus("Configuring PowerShell 7 profile...");
            progressReporter?.ReportProgress(60);

            var themeConfigPath = File.Exists(targetThemePath)
                ? targetThemePath.Replace("\\", "\\\\")  // Escape for string in profile
                : null;

            var profileContent = GenerateProfileContent(profilePath, themeConfigPath);
            File.WriteAllText(profilePath, profileContent);

            // Step 4: Configure Windows Terminal settings
            progressReporter?.ReportStatus("Configuring Windows Terminal settings...");
            progressReporter?.ReportProgress(80);

            var terminalConfigured = ConfigureWindowsTerminal(progressReporter);

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess("Oh My Posh installed and PowerShell profile configured!");
            progressReporter?.ReportStatus($"Profile: {profilePath}");
            progressReporter?.ReportStatus($"Theme:   {targetThemePath}");

            if (terminalConfigured)
            {
                progressReporter?.ReportSuccess("Windows Terminal configured (default shell: PowerShell 7, font: CaskaydiaMono Nerd Font)");
            }

            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Oh My Posh: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the PowerShell 7 (pwsh) profile path for the current user.
    /// Falls back to Documents\PowerShell\Microsoft.PowerShell_profile.ps1
    /// </summary>
    private static string? GetPwshProfilePath()
    {
        // Standard PowerShell 7 profile location on Windows
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documentsPath))
            return null;

        return Path.Combine(documentsPath, "PowerShell", "Microsoft.PowerShell_profile.ps1");
    }

    /// <summary>
    /// Finds and configures Windows Terminal settings.json with:
    /// - Default profile = PowerShell 7 (PowerShellCore)
    /// - Default font = CaskaydiaCove Nerd Font
    /// - Opacity = 70, useAcrylic = false
    /// - copyFormatting = none, copyOnSelect = false
    /// </summary>
    private static bool ConfigureWindowsTerminal(IProgressReporter? progressReporter)
    {
        try
        {
            var settingsPath = GetWindowsTerminalSettingsPath();
            if (settingsPath == null || !File.Exists(settingsPath))
            {
                progressReporter?.ReportWarning("Windows Terminal settings.json not found. Skipping terminal configuration.");
                return false;
            }

            var jsonText = File.ReadAllText(settingsPath);
            var jsonNode = JsonNode.Parse(jsonText, documentOptions: new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (jsonNode is not JsonObject root)
            {
                progressReporter?.ReportWarning("Could not parse Windows Terminal settings.json");
                return false;
            }

            // Set default profile to PowerShell Core (pwsh)
            root["defaultProfile"] = PowerShellCoreGuid;

            // Set copyFormatting and copyOnSelect
            root["copyFormatting"] = "none";
            root["copyOnSelect"] = false;

            // Configure profiles.defaults
            if (root["profiles"] is not JsonObject profiles)
            {
                profiles = new JsonObject();
                root["profiles"] = profiles;
            }

            var defaults = new JsonObject
            {
                ["font"] = new JsonObject
                {
                    ["face"] = "CaskaydiaMono Nerd Font"
                },
                ["opacity"] = 70,
                ["useAcrylic"] = false
            };
            profiles["defaults"] = defaults;

            // Write back with indentation
            var writeOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var updatedJson = jsonNode.ToJsonString(writeOptions);
            File.WriteAllText(settingsPath, updatedJson);

            progressReporter?.ReportStatus($"Terminal settings: {settingsPath}");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportWarning($"Could not configure Windows Terminal: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the Windows Terminal settings.json path.
    /// Checks both stable and preview versions.
    /// </summary>
    private static string? GetWindowsTerminalSettingsPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
            return null;

        // Stable version
        var stablePath = Path.Combine(localAppData,
            "Packages", "Microsoft.WindowsTerminal_8wekyb3d8bbwe", "LocalState", "settings.json");
        if (File.Exists(stablePath))
            return stablePath;

        // Preview version
        var previewPath = Path.Combine(localAppData,
            "Packages", "Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe", "LocalState", "settings.json");
        if (File.Exists(previewPath))
            return previewPath;

        return null;
    }

    /// <summary>
    /// Generates the PowerShell profile content, preserving any existing non-OMP/non-PSReadLine lines.
    /// </summary>
    private static string GenerateProfileContent(string profilePath, string? themeJsonPath)
    {
        var existingLines = new List<string>();
        if (File.Exists(profilePath))
        {
            existingLines = File.ReadAllLines(profilePath).ToList();
        }

        // Remove old oh-my-posh and PSReadLine config lines so we don't duplicate
        var preservedLines = existingLines
            .Where(line =>
                !line.TrimStart().StartsWith("oh-my-posh init", StringComparison.OrdinalIgnoreCase) &&
                !line.Contains("Invoke-Expression", StringComparison.OrdinalIgnoreCase) &&
                !line.TrimStart().StartsWith("Set-PSReadLineOption", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Remove trailing empty lines
        while (preservedLines.Count > 0 && string.IsNullOrWhiteSpace(preservedLines[^1]))
            preservedLines.RemoveAt(preservedLines.Count - 1);

        var sb = new System.Text.StringBuilder();

        // Add preserved lines first
        foreach (var line in preservedLines)
        {
            sb.AppendLine(line);
        }

        if (preservedLines.Count > 0)
            sb.AppendLine();

        // Add oh-my-posh init
        if (themeJsonPath != null)
        {
            // Use the copied theme file path (with proper escaping for PowerShell)
            var psPath = themeJsonPath.Replace("\\\\", "\\");
            sb.AppendLine($"oh-my-posh init pwsh --config \"{psPath}\" | Invoke-Expression");
        }
        else
        {
            // Fallback to built-in paradox theme
            sb.AppendLine("oh-my-posh init pwsh --config \"$env:POSH_THEMES_PATH\\paradox.omp.json\" | Invoke-Expression");
        }

        // Add PSReadLine configuration
        sb.AppendLine("Set-PSReadLineOption -PredictionSource History");
        sb.AppendLine("Set-PSReadLineOption -PredictionViewStyle ListView");

        return sb.ToString();
    }
}