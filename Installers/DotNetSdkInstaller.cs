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

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing .NET 8 SDK...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading .NET 8 SDK installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

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
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess(".NET 8 SDK installation completed successfully!");
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