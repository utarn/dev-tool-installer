namespace DevToolInstaller.Installers;

public class WireGuardInstaller : IInstaller
{
    public string Name => "WireGuard";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Fast, modern, secure VPN tunnel";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("wireguard.exe"))
        {
            return true;
        }

        // Check common install locations
        var possiblePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WireGuard", "wireguard.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WireGuard", "wireguard.exe"),
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing WireGuard...");

        try
        {
            // Option 1 (preferred): Install via winget
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing WireGuard via winget...");
                progressReporter?.ReportProgress(20);

                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=WireGuard.WireGuard -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("WireGuard installation completed successfully!");
                    return true;
                }

                progressReporter?.ReportWarning("winget installation failed, trying bundled installer...");
            }

            // Option 2 (fallback): Use bundled wireguard-installer.exe
            var appDir = Path.GetDirectoryName(Environment.ProcessPath) ?? ".";
            var bundledInstaller = Path.Combine(appDir, "wireguard-installer.exe");

            if (File.Exists(bundledInstaller))
            {
                progressReporter?.ReportStatus("Installing WireGuard via bundled installer...");
                progressReporter?.ReportProgress(50);

                var success = ProcessHelper.ExecuteInstaller(bundledInstaller, "/S");

                if (success)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("WireGuard installation completed successfully!");
                    return true;
                }
            }
            else
            {
                progressReporter?.ReportWarning("Bundled installer not found at: " + bundledInstaller);
            }

            progressReporter?.ReportError("WireGuard installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install WireGuard: {ex.Message}");
            return false;
        }
    }
}