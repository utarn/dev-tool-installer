namespace DevToolInstaller.Installers;

public class NodeJs20Installer : IInstaller
{
    private const string DownloadUrl = "https://nodejs.org/dist/v20.12.2/node-v20.12.2-x64.msi";
    private const string InstallerFileName = "nodejs20-installer.msi";

    public string Name => "Node.js 20";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js 20 JavaScript runtime with npm included";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("node.exe") && 
               await ProcessHelper.FindExecutableInPathAsync("npm.cmd");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Node.js 20...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading Node.js 20 installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running Node.js 20 installer...");
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
                progressReporter?.ReportSuccess("Node.js 20 installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Node.js 20 installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Node.js 20: {ex.Message}");
            return false;
        }
    }
}