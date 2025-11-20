namespace DevToolInstaller.Installers;

public class NgrokInstaller : IInstaller
{
    private const string DownloadUrl = "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip";
    private const string ZipFileName = "ngrok.zip";

    public string Name => "Ngrok";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Secure tunneling service for exposing local services to the internet";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("ngrok.exe") || ProcessHelper.IsToolInstalled("ngrok"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Ngrok...");

        var tempPath = Path.GetTempPath();
        var zipPath = Path.Combine(tempPath, ZipFileName);

        try
        {
            // Try winget first
            if (ProcessHelper.IsToolInstalled("winget"))
            {
                progressReporter?.ReportStatus("Installing Ngrok via winget...");
                progressReporter?.ReportProgress(30);
                var output = ProcessHelper.GetCommandOutput("winget",
                    "install --id=ngrok.ngrok -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("Ngrok installation completed successfully!");
                    return true;
                }
            }

            // Fallback to direct download and extract
            progressReporter?.ReportStatus("Downloading Ngrok...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, zipPath, Name, progressReporter, cancellationToken);

            var extractPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "ngrok");

            progressReporter?.ReportStatus("Extracting Ngrok...");
            progressReporter?.ReportProgress(60);
            
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath, true);

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(80);
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            // Add to PATH for current session
            progressReporter?.ReportStatus("Adding to PATH...");
            progressReporter?.ReportProgress(90);
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            if (!currentPath.Contains(extractPath))
            {
                Environment.SetEnvironmentVariable("PATH", $"{currentPath};{extractPath}", EnvironmentVariableTarget.User);
                progressReporter?.ReportSuccess($"Added {extractPath} to user PATH");
            }

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess("Ngrok installation completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Ngrok: {ex.Message}");
            return false;
        }
    }
}