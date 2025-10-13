namespace DevToolInstaller.Installers;

public class PowerShell7Installer : IInstaller
{
    private const string DownloadUrl = "https://github.com/PowerShell/PowerShell/releases/download/v7.4.5/PowerShell-7.4.5-win-x64.msi";
    private const string InstallerFileName = "PowerShell7Setup.msi";

    public string Name => "PowerShell 7";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Cross-platform automation and configuration tool/framework";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("pwsh.exe") || ProcessHelper.IsToolInstalled("pwsh"))
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
            // Try winget first
            if (ProcessHelper.IsToolInstalled("winget"))
            {
                ConsoleHelper.WriteInfo($"Installing {Name} via winget...");
                var output = ProcessHelper.GetCommandOutput("winget",
                    "install --id=Microsoft.PowerShell -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                    return true;
                }
            }

            // Fallback to direct download
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, cancellationToken);

            ConsoleHelper.WriteInfo($"Running {Name} installer...");
            var success = ProcessHelper.ExecuteMsiInstaller(installerPath);

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