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
            return true;
        }

        // Check common Python Scripts directories where pip installs executables
        var searchBases = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python"),
            @"C:\Python",
        };

        foreach (var basePath in searchBases)
        {
            if (!Directory.Exists(basePath)) continue;
            try
            {
                foreach (var pyDir in Directory.GetDirectories(basePath, "Python*"))
                {
                    if (File.Exists(Path.Combine(pyDir, "Scripts", "poetry.exe")))
                    {
                        return true;
                    }
                }
            }
            catch { /* permission denied etc. */ }
        }

        // Also check user-level pip install location
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userScripts = Path.Combine(appData, "Python", "Scripts", "poetry.exe");
        if (File.Exists(userScripts))
        {
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