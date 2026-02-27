namespace DevToolInstaller.Installers;

/// <summary>
/// Configures .wslconfig to limit WSL2 memory and swap usage.
/// File: %USERPROFILE%\.wslconfig
/// </summary>
public class WslConfigInstaller : IInstaller
{
    public string Name => "WSL2 Memory Limit";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Configure WSL2: limit memory to 4GB, swap to 8GB (.wslconfig)";
    public List<string> Dependencies => new();

    private static string WslConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".wslconfig");

    private const string WslConfigContent = """
        [wsl2]
        memory=4GB
        swap=8GB
        localhostForwarding=true
        """;

    public Task<bool> IsInstalledAsync()
    {
        try
        {
            if (File.Exists(WslConfigPath))
            {
                var content = File.ReadAllText(WslConfigPath);
                if (content.Contains("memory=4GB") && content.Contains("swap=8GB"))
                {
                    return Task.FromResult(true);
                }
            }
        }
        catch
        {
            // If we can't read, assume not configured
        }

        return Task.FromResult(false);
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing/updating WSL...");
        progressReporter?.ReportProgress(5);

        try
        {
            // Step 1: Install WSL if not already enabled (no-distribution = don't install Ubuntu)
            progressReporter?.ReportStatus("Enabling WSL2 (wsl --install --no-distribution)...");
            await ProcessHelper.GetCommandOutput("wsl", "--install --no-distribution");
            progressReporter?.ReportProgress(30);

            // Step 2: Update WSL to latest version
            progressReporter?.ReportStatus("Updating WSL to latest version...");
            await ProcessHelper.GetCommandOutput("wsl", "--update");
            progressReporter?.ReportStatus("WSL updated to latest version");
            progressReporter?.ReportProgress(40);

            // Build the config content
            var lines = new List<string>();

            if (File.Exists(WslConfigPath))
            {
                // Read existing config and update/add settings
                progressReporter?.ReportStatus("Reading existing .wslconfig...");
                progressReporter?.ReportProgress(50);
                
                var existingLines = File.ReadAllLines(WslConfigPath).ToList();
                bool inWsl2Section = false;
                bool hasMemory = false;
                bool hasSwap = false;
                bool hasLocalhost = false;
                bool hasWsl2Header = false;

                for (int i = 0; i < existingLines.Count; i++)
                {
                    var trimmed = existingLines[i].Trim();

                    if (trimmed.StartsWith("[wsl2]", StringComparison.OrdinalIgnoreCase))
                    {
                        inWsl2Section = true;
                        hasWsl2Header = true;
                        lines.Add(existingLines[i]);
                        continue;
                    }

                    if (trimmed.StartsWith("[") && !trimmed.StartsWith("[wsl2]", StringComparison.OrdinalIgnoreCase))
                    {
                        // Entering a different section — inject missing keys before leaving [wsl2]
                        if (inWsl2Section)
                        {
                            if (!hasMemory) lines.Add("memory=4GB");
                            if (!hasSwap) lines.Add("swap=8GB");
                            if (!hasLocalhost) lines.Add("localhostForwarding=true");
                        }
                        inWsl2Section = false;
                        lines.Add(existingLines[i]);
                        continue;
                    }

                    if (inWsl2Section)
                    {
                        if (trimmed.StartsWith("memory=", StringComparison.OrdinalIgnoreCase))
                        {
                            lines.Add("memory=4GB");
                            hasMemory = true;
                            continue;
                        }
                        if (trimmed.StartsWith("swap=", StringComparison.OrdinalIgnoreCase))
                        {
                            lines.Add("swap=8GB");
                            hasSwap = true;
                            continue;
                        }
                        if (trimmed.StartsWith("localhostForwarding=", StringComparison.OrdinalIgnoreCase))
                        {
                            lines.Add("localhostForwarding=true");
                            hasLocalhost = true;
                            continue;
                        }
                    }

                    lines.Add(existingLines[i]);
                }

                // If we were still in [wsl2] section at end of file, add missing keys
                if (inWsl2Section)
                {
                    if (!hasMemory) lines.Add("memory=4GB");
                    if (!hasSwap) lines.Add("swap=8GB");
                    if (!hasLocalhost) lines.Add("localhostForwarding=true");
                }

                // If [wsl2] section didn't exist at all
                if (!hasWsl2Header)
                {
                    lines.Add("");
                    lines.Add("[wsl2]");
                    lines.Add("memory=4GB");
                    lines.Add("swap=8GB");
                    lines.Add("localhostForwarding=true");
                }
            }
            else
            {
                // Create new .wslconfig
                progressReporter?.ReportStatus("Creating new .wslconfig...");
                progressReporter?.ReportProgress(50);
                
                lines.Add("[wsl2]");
                lines.Add("memory=4GB");
                lines.Add("swap=8GB");
                lines.Add("localhostForwarding=true");
            }

            progressReporter?.ReportStatus("Writing .wslconfig...");
            progressReporter?.ReportProgress(60);
            File.WriteAllLines(WslConfigPath, lines);

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess(
                $"WSL2 configured: memory=4GB, swap=8GB\n" +
                $"  File: {WslConfigPath}\n" +
                "  Restart WSL for changes to take effect: wsl --shutdown");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to configure WSL2: {ex.Message}");
            return false;
        }
    }
}