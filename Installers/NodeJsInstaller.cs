namespace DevToolInstaller.Installers;

public class NodeJsInstaller : IInstaller
{
    private const string NodeVersion = "20.19.6";

    public string Name => "Node.js";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js JavaScript runtime with npm included";
    public List<string> Dependencies => new() { "NVM for Windows" };

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("node.exe");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Node.js via nvm...");

        try
        {
            progressReporter?.ReportStatus($"Installing Node.js {NodeVersion}...");
            progressReporter?.ReportProgress(40);
            var installSuccess = await ProcessHelper.ExecuteCommand("nvm", $"install {NodeVersion}");

            if (!installSuccess)
            {
                progressReporter?.ReportError("Node.js installation failed");
                return false;
            }

            progressReporter?.ReportStatus($"Activating Node.js {NodeVersion}...");
            progressReporter?.ReportProgress(75);
            var useSuccess = await ProcessHelper.ExecuteCommand("nvm", $"use {NodeVersion}");

            if (!useSuccess)
            {
                progressReporter?.ReportError("Failed to activate Node.js");
                return false;
            }

            progressReporter?.ReportStatus("Refreshing environment variables...");
            progressReporter?.ReportProgress(95);
            ProcessHelper.RefreshEnvironmentVariables();

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess("Node.js installation completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Node.js: {ex.Message}");
            return false;
        }
    }
}