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

        var output = await ProcessHelper.GetCommandOutput("dotnet", "--list-sdks");
        if (output != null && output.Contains("10.0."))
        {
            return true;
        }
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing .NET 10 SDK...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading .NET 10 SDK installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running .NET 10 SDK installer...");
            progressReporter?.ReportProgress(50);
            var success = ProcessHelper.ExecuteInstaller(installerPath, "/quiet /norestart");

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(90);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                progressReporter?.ReportStatus("Refreshing environment variables...");
                progressReporter?.ReportProgress(95);
                ProcessHelper.RefreshEnvironmentVariables();
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess(".NET 10 SDK installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError(".NET 10 SDK installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install .NET 10 SDK: {ex.Message}");
            return false;
        }
    }
}