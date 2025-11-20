namespace DevToolInstaller.Installers;

public class PoetryInstaller : IInstaller
{
    public string Name => "Poetry";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Python dependency management and packaging tool";
    public List<string> Dependencies => new() { "Python", "Pip" };

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("poetry.exe"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Poetry...");

        try
        {
            // Ensure pip is up to date
            progressReporter?.ReportStatus("Updating pip...");
            progressReporter?.ReportProgress(10);
            await ProcessHelper.ExecuteCommand("python", "-m pip install --upgrade pip");
            
            progressReporter?.ReportStatus("Installing Poetry using pip...");
            progressReporter?.ReportProgress(30);
            var success = await ProcessHelper.ExecuteCommand("pip", "install poetry");

            if (success)
            {
                progressReporter?.ReportStatus("Refreshing environment variables...");
                progressReporter?.ReportProgress(90);
                ProcessHelper.RefreshEnvironmentVariables();
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Poetry installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Poetry installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Poetry: {ex.Message}");
            return false;
        }
    }
}