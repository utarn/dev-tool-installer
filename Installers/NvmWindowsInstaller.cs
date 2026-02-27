namespace DevToolInstaller.Installers;

public class NvmWindowsInstaller : IInstaller
{
    public string Name => "NVM for Windows";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js version manager for Windows (nvm-windows v1.2.2)";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("nvm.exe"))
        {
            return true;
        }

        var output = await ProcessHelper.GetCommandOutput("nvm", "version");
        return !string.IsNullOrWhiteSpace(output);
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing NVM for Windows...");

        try
        {
            if (!await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportWarning("winget not found. Please install NVM for Windows manually.");
                return false;
            }

            progressReporter?.ReportStatus("Installing NVM for Windows via winget...");
            progressReporter?.ReportProgress(30);

            var output = await ProcessHelper.GetCommandOutput(
                "winget",
                "install --id=CoreyButler.NVMforWindows -e --source=winget --silent --accept-source-agreements --accept-package-agreements --force");

            if (output is null)
            {
                progressReporter?.ReportError("NVM for Windows installation failed");
                return false;
            }

            progressReporter?.ReportStatus("Refreshing environment variables...");
            progressReporter?.ReportProgress(90);
            ProcessHelper.RefreshEnvironmentVariables();

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess("NVM for Windows installation completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install NVM for Windows: {ex.Message}");
            return false;
        }
    }
}