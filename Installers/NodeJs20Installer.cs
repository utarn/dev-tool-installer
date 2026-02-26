namespace DevToolInstaller.Installers;

public class NodeJs20Installer : IInstaller
{
    private const string NodeVersion = "20.19.6";

    public string Name => "Node.js 20";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js 20 JavaScript runtime with npm included";
    public List<string> Dependencies => new() { "NVM for Windows" };

    public async Task<bool> IsInstalledAsync()
    {
        var nvmList = await ProcessHelper.GetCommandOutput("nvm", "list");
        if (string.IsNullOrWhiteSpace(nvmList) || !nvmList.Contains(NodeVersion))
        {
            return false;
        }

        return await ProcessHelper.FindExecutableInPathAsync("node.exe") &&
               await ProcessHelper.GetCommandOutput("node", "--version") is not null;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Node.js 20 via nvm...");

        try
        {
            progressReporter?.ReportStatus($"Installing Node.js {NodeVersion}...");
            progressReporter?.ReportProgress(40);
            var installSuccess = await ProcessHelper.ExecuteCommand("nvm", $"install {NodeVersion}");

            if (!installSuccess)
            {
                progressReporter?.ReportError("Node.js 20 installation failed");
                return false;
            }

            progressReporter?.ReportStatus($"Activating Node.js {NodeVersion}...");
            progressReporter?.ReportProgress(75);
            var useSuccess = await ProcessHelper.ExecuteCommand("nvm", $"use {NodeVersion}");

            if (!useSuccess)
            {
                progressReporter?.ReportError("Failed to activate Node.js 20");
                return false;
            }

            progressReporter?.ReportStatus("Refreshing environment variables...");
            progressReporter?.ReportProgress(95);
            ProcessHelper.RefreshEnvironmentVariables();

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess("Node.js 20 installation completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Node.js 20: {ex.Message}");
            return false;
        }
    }
}