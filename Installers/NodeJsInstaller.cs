namespace DevToolInstaller.Installers;

public class NodeJsInstaller : IInstaller
{
    private const string DownloadUrl = "https://nodejs.org/dist/v20.12.2/node-v20.12.2-x64.msi";
    private const string InstallerFileName = "nodejs-installer.msi";

    public string Name => "Node.js";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js JavaScript runtime with npm included";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("node.exe") && 
               await ProcessHelper.FindExecutableInPathAsync("npm.cmd");
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
            var success = ProcessHelper.ExecuteMsiInstaller(installerPath, "/quiet /norestart ADDLOCAL=ALL");

            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                ProcessHelper.RefreshEnvironmentVariables();
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