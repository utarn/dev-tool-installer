namespace DevToolInstaller.Installers;

public class PowerShell7Installer : IInstaller
{
    private const string DownloadUrl = "https://github.com/PowerShell/PowerShell/releases/download/v7.5.4/PowerShell-7.5.4-win-x64.msi";
    private const string InstallerFileName = "PowerShell7Setup.msi";

    public string Name => "PowerShell 7";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Cross-platform automation and configuration tool/framework";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("pwsh.exe") || ProcessHelper.IsToolInstalled("pwsh"))
        {
            return true;
        }
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing PowerShell 7...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            // Try winget first
            if (ProcessHelper.IsToolInstalled("winget"))
            {
                progressReporter?.ReportStatus("Installing PowerShell 7 via winget...");
                progressReporter?.ReportProgress(20);
                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=Microsoft.PowerShell -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("PowerShell 7 installation completed successfully!");
                    return true;
                }
            }

            // Fallback to direct download
            progressReporter?.ReportStatus("Downloading PowerShell 7 installer...");
            progressReporter?.ReportProgress(30);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running PowerShell 7 installer...");
            progressReporter?.ReportProgress(70);
            var success = ProcessHelper.ExecuteMsiInstaller(installerPath);

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(90);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("PowerShell 7 installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("PowerShell 7 installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install PowerShell 7: {ex.Message}");
            return false;
        }
    }
}