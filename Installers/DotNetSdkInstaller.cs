using System.Runtime.InteropServices;

namespace DevToolInstaller.Installers;

public class DotNetSdkInstaller : IInstaller
{
    // Fallback version if API call fails
    private const string FallbackVersion = "8.0.414";
    
    private string _downloadUrl = string.Empty;
    private string _installerFileName = string.Empty;
    private string _version = string.Empty;

    public string Name => ".NET 8 SDK";
    public DevelopmentCategory Category => DevelopmentCategory.CSharp;
    public string Description => ".NET development framework for building modern applications";
    public List<string> Dependencies => new();

    public DotNetSdkInstaller()
    {
        InitializeInstallerDetails();
    }

    private async void InitializeInstallerDetails()
    {
        await InitializeVersionAsync();
    }

    private async Task InitializeVersionAsync()
    {
        var versionInfo = await VersionHelper.GetLatestDotNetSdkVersionAsync("8.0");
        
        if (versionInfo.HasValue)
        {
            _version = versionInfo.Value.Version;
            _downloadUrl = versionInfo.Value.DownloadUrl;
        }
        else
        {
            // Fallback to hardcoded version
            _version = FallbackVersion;
            var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
            _downloadUrl = $"https://builds.dotnet.microsoft.com/dotnet/Sdk/{FallbackVersion}/dotnet-sdk-{FallbackVersion}-win-{arch}.exe";
        }
        
        _installerFileName = $"dotnet-sdk-{_version}-win-x64.exe";
    }

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

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // Ensure version is initialized
        if (string.IsNullOrEmpty(_downloadUrl))
        {
            await InitializeVersionAsync();
        }

        progressReporter?.ReportStatus("Installing .NET 8 SDK...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, _installerFileName);

        try
        {
            progressReporter?.ReportStatus($"Downloading .NET 8 SDK {_version}...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(_downloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running .NET 8 SDK installer...");
            progressReporter?.ReportProgress(50);
            var success = ProcessHelper.ExecuteInstaller(installerPath, "/install /quiet /norestart");

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
                progressReporter?.ReportSuccess($".NET 8 SDK {_version} installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError(".NET 8 SDK installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install .NET 8 SDK: {ex.Message}");
            return false;
        }
    }
}