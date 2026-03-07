namespace DevToolInstaller.Installers;

public class PythonInstaller : IInstaller
{
    public string Name => "Python (via uv)";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Python programming language installed via uv (fast package manager)";
    public List<string> Dependencies => new() { "uv" };

    public async Task<bool> IsInstalledAsync()
    {
        // Check if Python is installed via uv or in PATH
        if (await ProcessHelper.FindExecutableInPathAsync("python.exe") || ProcessHelper.IsToolInstalled("python"))
        {
            return true;
        }
        
        // Check if uv has Python installed
        var uvPythonOutput = await ProcessHelper.GetCommandOutput("uv", "python list");
        if (!string.IsNullOrWhiteSpace(uvPythonOutput) && uvPythonOutput.Contains("cpython"))
        {
            return true;
        }
        
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Python via uv...");

        try
        {
            // First check if uv is available
            if (!await ProcessHelper.FindExecutableInPathAsync("uv.exe"))
            {
                progressReporter?.ReportWarning("uv not found. Installing uv first...");
                var uvInstaller = new UvInstaller();
                var uvInstalled = await uvInstaller.InstallAsync(progressReporter, cancellationToken);
                
                if (!uvInstalled)
                {
                    progressReporter?.ReportError("Failed to install uv. Cannot proceed with Python installation.");
                    return false;
                }
                
                // Refresh environment variables after uv installation
                ProcessHelper.RefreshEnvironmentVariables();
            }

            // Install latest Python version using uv
            progressReporter?.ReportStatus("Installing latest Python version via uv...");
            progressReporter?.ReportProgress(30);

            // Use uv to install the latest Python version
            // uv python install will install the latest stable Python
            var installSuccess = await ProcessHelper.ExecuteCommand("uv", "python install");

            if (!installSuccess)
            {
                progressReporter?.ReportError("Python installation via uv failed");
                return false;
            }

            progressReporter?.ReportProgress(70);
            
            // Refresh environment variables
            progressReporter?.ReportStatus("Refreshing environment variables...");
            ProcessHelper.RefreshEnvironmentVariables();
            progressReporter?.ReportProgress(90);

            // Verify installation
            var pythonVersion = await ProcessHelper.GetCommandOutput("uv", "python list");
            if (!string.IsNullOrWhiteSpace(pythonVersion))
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess($"Python installation completed successfully!{Environment.NewLine}Installed versions:{Environment.NewLine}{pythonVersion}");
                return true;
            }

            progressReporter?.ReportError("Python installation completed but verification failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Python: {ex.Message}");
            return false;
        }
    }
}