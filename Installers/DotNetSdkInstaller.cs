namespace DevToolInstaller.Installers;

public class DotNetSdkInstaller : IInstaller
{
    private const string DownloadUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.414/dotnet-sdk-8.0.414-win-x64.exe";
    private const string InstallerFileName = "dotnet-sdk-8.0.414-win-x64.exe";

    public string Name => ".NET 8 SDK";
    public DevelopmentCategory Category => DevelopmentCategory.CSharp;
    public string Description => ".NET development framework for building modern applications";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (!await ProcessHelper.FindExecutableInPathAsync("dotnet.exe"))
        {
            return false;
        }
        
        var output = await ProcessHelper.GetCommandOutput("dotnet", "--info");
        if (output != null && output.Contains("8.0."))
        {
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
            var success = ProcessHelper.ExecuteInstaller(installerPath, "/install /quiet /norestart");

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