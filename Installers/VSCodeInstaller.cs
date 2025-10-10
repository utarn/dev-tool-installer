namespace DevToolInstaller.Installers;

public class VSCodeInstaller : IInstaller
{
    private const string DownloadUrl = "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user";
    private const string InstallerFileName = "VSCodeSetup.exe";

    private static readonly string[] Extensions = 
    [
        "modelharbor.modelharbor-agent",
        "ms-dotnettools.vscode-dotnet-runtime",
        "formulahendry.dotnet",
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit",
        "ms-dotnettools.vscodeintellicode-csharp",
        "alexcvzz.vscode-sqlite"
    ];

    public string Name => "Visual Studio Code";

    public Task<bool> IsInstalledAsync()
    {
        if (ProcessHelper.IsToolInstalled("code"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
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
            var success = ProcessHelper.ExecuteInstaller(installerPath, "/VERYSILENT /NORESTART /MERGETASKS=!runcode");

            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                
                // Install extensions
                await InstallExtensionsAsync();
                
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

    private async Task InstallExtensionsAsync()
    {
        ConsoleHelper.WriteInfo("Installing VS Code extensions...");
        
        foreach (var extension in Extensions)
        {
            try
            {
                ConsoleHelper.WriteInfo($"Installing extension: {extension}");
                ProcessHelper.GetCommandOutput("code", $"--install-extension {extension}");
                await Task.Delay(1000); // Brief delay between installations
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteWarning($"Failed to install extension {extension}: {ex.Message}");
            }
        }
        
        ConsoleHelper.WriteSuccess("VS Code extensions installation completed!");
    }
}