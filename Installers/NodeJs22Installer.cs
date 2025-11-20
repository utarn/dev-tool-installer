namespace DevToolInstaller.Installers;

public class NodeJs22Installer : IInstaller
{
    private const string DownloadUrl = "https://nodejs.org/dist/v22.11.0/node-v22.11.0-x64.msi";
    private const string InstallerFileName = "nodejs22-installer.msi";

    public string Name => "Node.js 22";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js 22 JavaScript runtime with npm included";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("node.exe") && 
               await ProcessHelper.FindExecutableInPathAsync("npm.cmd");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Node.js 22...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading Node.js 22 installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running Node.js 22 installer...");
            progressReporter?.ReportProgress(50);
            var success = ProcessHelper.ExecuteMsiInstaller(installerPath, "/quiet /norestart ADDLOCAL=ALL");

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(90);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                progressReporter?.ReportStatus("Refreshing environment variables...");
                progressReporter?.ReportProgress(95);
                ProcessHelper.RefreshEnvironmentVariables();
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Node.js 22 installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Node.js 22 installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Node.js 22: {ex.Message}");
            return false;
        }
    }
}