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

    public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, cancellationToken);

            ConsoleHelper.WriteInfo($"Running {Name} installer...");
            // Install Python with pip and add to PATH
            var arguments = "/quiet InstallAllUsers=1 PrependPath=1 Include_test=0";
            var success = ProcessHelper.ExecuteInstaller(installerPath, arguments);

            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                return true;
            }
            else
            {
                ConsoleHelper.WriteError($"{Name} installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to install {Name}: {ex.Message}");
            return false;
        }
    }
}