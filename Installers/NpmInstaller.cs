namespace DevToolInstaller.Installers;

public class NpmInstaller : IInstaller
{
    public string Name => "NPM";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node Package Manager - Package manager for JavaScript";
    public List<string> Dependencies => new() { "Node.js 20" };

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("npm.cmd");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing NPM...");

        try
        {
            // Update npm to the latest version
            progressReporter?.ReportStatus("Updating npm to the latest version...");
            progressReporter?.ReportProgress(20);
            
            var success = await ProcessHelper.ExecuteCommand("npm", "install -g npm@latest");

            if (success)
            {
                progressReporter?.ReportStatus("Refreshing environment variables...");
                progressReporter?.ReportProgress(80);
                ProcessHelper.RefreshEnvironmentVariables();
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("NPM installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("NPM installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install NPM: {ex.Message}");
            return false;
        }
    }
}