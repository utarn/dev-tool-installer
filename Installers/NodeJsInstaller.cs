namespace DevToolInstaller.Installers;

public class NodeJsInstaller : IInstaller
{
    private const string DownloadUrl = "https://nodejs.org/dist/v20.12.2/node-v20.12.2-x64.msi";
    private const string InstallerFileName = "nodejs-installer.msi";

    public string Name => "Node.js";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js JavaScript runtime with npm included";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("node.exe") && 
               await ProcessHelper.FindExecutableInPathAsync("npm.cmd");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Node.js...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading Node.js installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running Node.js installer...");
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
                progressReporter?.ReportSuccess("Node.js installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Node.js installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Node.js: {ex.Message}");
            return false;
        }
    }
}