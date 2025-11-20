namespace DevToolInstaller.Installers;

public class PythonInstaller : IInstaller
{
    private const string DownloadUrl = "https://www.python.org/ftp/python/3.12.5/python-3.12.5-amd64.exe";
    private const string InstallerFileName = "python-installer.exe";

    public string Name => "Python";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Python programming language interpreter and standard library";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("python.exe") || ProcessHelper.IsToolInstalled("python"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Python...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading Python installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running Python installer...");
            progressReporter?.ReportProgress(50);
            // Install Python with pip and add to PATH
            var arguments = "/quiet InstallAllUsers=1 PrependPath=1 Include_test=0";
            var success = ProcessHelper.ExecuteInstaller(installerPath, arguments);

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(90);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Python installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Python installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Python: {ex.Message}");
            return false;
        }
    }
}