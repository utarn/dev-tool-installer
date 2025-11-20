namespace DevToolInstaller.Installers;

public class PipInstaller : IInstaller
{
    public string Name => "Pip";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Python package installer and manager";
    public List<string> Dependencies => new() { "Python" };

    public async Task<bool> IsInstalledAsync()
    {
        // Primary check: Check if pip is available by running python -m pip --version
        try
        {
            var result = await ProcessHelper.GetCommandOutput("python", "-m pip --version");
            if (!string.IsNullOrWhiteSpace(result))
            {
                return true;
            }
        }
        catch
        {
            // Ignore exception, proceed to secondary check
        }
        
        // Secondary check: Check for pip executable in PATH (e.g., pip.exe)
        if (await ProcessHelper.FindExecutableInPathAsync("pip.exe"))
        {
            return true;
        }
        
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Pip...");

        try
        {
            // First try to ensure pip is installed
            progressReporter?.ReportStatus("Ensuring pip is installed...");
            progressReporter?.ReportProgress(20);
            var ensureSuccess = await ProcessHelper.ExecuteCommand("python", "-m ensurepip --default-pip");
             
            if (!ensureSuccess)
            {
                progressReporter?.ReportWarning("ensurepip failed, trying alternative installation method...");
            }

            // Upgrade pip to the latest version
            progressReporter?.ReportStatus("Upgrading pip to the latest version...");
            progressReporter?.ReportProgress(60);
            var upgradeSuccess = await ProcessHelper.ExecuteCommand("python", "-m pip install --upgrade pip");

            if (upgradeSuccess)
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Pip installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Pip installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Pip: {ex.Message}");
            return false;
        }
    }
}