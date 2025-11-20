namespace DevToolInstaller.Installers;

public class GitInstaller : IInstaller
{
    private const string DownloadUrl = "https://github.com/git-for-windows/git/releases/download/v2.46.0.windows.1/Git-2.46.0-64-bit.exe";
    private const string InstallerFileName = "GitSetup.exe";

    public string Name => "Git";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Distributed version control system for tracking changes in source code";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("git.exe");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Git...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing Git via winget...");
                progressReporter?.ReportProgress(20);
                var success = await ProcessHelper.ExecuteCommand("winget",
                    "install --id Git.Git -e --source winget --accept-source-agreements --accept-package-agreements");
                 
                if (success)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("Git installation completed successfully!");
                    return true;
                }
                else
                {
                    progressReporter?.ReportWarning("Git installation failed via winget, trying direct download...");
                    // Fallback to direct download if winget fails
                }
            }

            // Fallback to direct download and install
            progressReporter?.ReportStatus("Downloading Git installer...");
            progressReporter?.ReportProgress(30);
            var tempPath = Path.GetTempPath();
            var installerPath = Path.Combine(tempPath, InstallerFileName);

            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running Git installer...");
            progressReporter?.ReportProgress(70);
            var successDirect = ProcessHelper.ExecuteInstaller(installerPath, "/VERYSILENT /NORESTART");

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(90);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (successDirect)
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Git installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Git installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Git: {ex.Message}");
            return false;
        }
    }
}