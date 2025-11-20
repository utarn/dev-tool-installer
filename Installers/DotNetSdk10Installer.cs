using System.Runtime.InteropServices;

namespace DevToolInstaller.Installers;

public class DotNetSdk10Installer : IInstaller
{
    private string DownloadUrl = string.Empty;
    private string InstallerFileName = string.Empty;

    public string Name => ".NET 10 SDK";
    public DevelopmentCategory Category => DevelopmentCategory.CSharp;
    public string Description => ".NET 10.0 development framework for building modern applications";
    public List<string> Dependencies => new();

    public DotNetSdk10Installer()
    {
        InitializeInstallerDetails();
    }

    private void InitializeInstallerDetails()
    {
        if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
        {
            DownloadUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.100/dotnet-sdk-10.0.100-win-arm64.exe";
            InstallerFileName = "dotnet-sdk-10.0.100-win-arm64.exe";
        }
        else // Default to x64
        {
            DownloadUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.100/dotnet-sdk-10.0.100-win-x64.exe";
            InstallerFileName = "dotnet-sdk-10.0.100-win-x64.exe";
        }
    }

    public async Task<bool> IsInstalledAsync()
    {
        if (!await ProcessHelper.FindExecutableInPathAsync("dotnet.exe"))
        {
            return false;
        }

        var output = ProcessHelper.GetCommandOutput("dotnet", "--list-sdks");
        if (output != null && output.Contains("10.0."))
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
            var success = ProcessHelper.ExecuteInstaller(installerPath, "/quiet /norestart");

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