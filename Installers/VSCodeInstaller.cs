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
    public DevelopmentCategory Category => DevelopmentCategory.CSharp;
    public string Description => "Lightweight but powerful source code editor with extensive extension support";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("code.exe") || ProcessHelper.IsToolInstalled("code");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Visual Studio Code...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading VS Code installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running VS Code installer...");
            progressReporter?.ReportProgress(50);
            var success = ProcessHelper.ExecuteInstaller(installerPath, "/VERYSILENT /NORESTART /MERGETASKS=!runcode");

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(80);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                progressReporter?.ReportStatus("Installing extensions...");
                progressReporter?.ReportProgress(90);
                // Install extensions
                await InstallExtensionsAsync(progressReporter);
                
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Visual Studio Code installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Visual Studio Code installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Visual Studio Code: {ex.Message}");
            return false;
        }
    }

    private async Task InstallExtensionsAsync(IProgressReporter? progressReporter = null)
    {
        progressReporter?.ReportStatus("Installing VS Code extensions...");
        
        var totalExtensions = Extensions.Length;
        for (int i = 0; i < totalExtensions; i++)
        {
            var extension = Extensions[i];
            try
            {
                progressReporter?.ReportStatus($"Installing extension: {extension}");
                var progress = 90 + (i * 10 / totalExtensions);
                progressReporter?.ReportProgress(progress);
                await ProcessHelper.GetCommandOutput("code", $"--install-extension {extension}");
                await Task.Delay(1000); // Brief delay between installations
            }
            catch (Exception ex)
            {
                progressReporter?.ReportWarning($"Failed to install extension {extension}: {ex.Message}");
            }
        }
        
        progressReporter?.ReportSuccess("VS Code extensions installation completed!");
    }
}