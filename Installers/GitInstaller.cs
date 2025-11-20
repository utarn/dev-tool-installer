namespace DevToolInstaller.Installers;

public class GitInstaller : IInstaller
{
    private const string DownloadUrl = "https://github.com/git-for-windows/git/releases/download/v2.46.0.windows.1/Git-2.46.0-64-bit.exe";
    private const string InstallerFileName = "GitSetup.exe";

    public string Name => "Git";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Distributed version control system for tracking changes in source code";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("git.exe");
    }

    public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                ConsoleHelper.WriteInfo($"Installing {Name} via winget...");
                var success = await ProcessHelper.ExecuteCommand("winget",
                    "install --id Git.Git -e --source winget --accept-source-agreements --accept-package-agreements");
                
                if (success)
                {
                    ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                    return true;
                }
                else
                {
                    ConsoleHelper.WriteError($"{Name} installation failed via winget");
                    // Fallback to direct download if winget fails
                }
            }

            // Fallback to direct download and install
            ConsoleHelper.WriteInfo("Downloading Git installer...");
            var tempPath = Path.GetTempPath();
            var installerPath = Path.Combine(tempPath, InstallerFileName);

            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, cancellationToken);

            ConsoleHelper.WriteInfo($"Running {Name} installer...");
            var successDirect = ProcessHelper.ExecuteInstaller(installerPath, "/VERYSILENT /NORESTART");

            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (successDirect)
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