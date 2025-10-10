namespace DevToolInstaller.Installers;

public class DotNetSdkInstaller : IInstaller
{
    private const string DownloadUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.414/dotnet-sdk-8.0.414-win-x64.exe";
    private const string InstallerFileName = "dotnet-sdk-8.0.414-win-x64.exe";

    public string Name => ".NET 8 SDK";

    public Task<bool> IsInstalledAsync()
    {
        var output = ProcessHelper.GetCommandOutput("dotnet", "--info");
        if (output != null && output.Contains("8.0."))
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