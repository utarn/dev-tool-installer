namespace DevToolInstaller.Installers;

public class NgrokInstaller : IInstaller
{
    private const string DownloadUrl = "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip";
    private const string ZipFileName = "ngrok.zip";

    public string Name => "Ngrok";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Secure tunneling service for exposing local services to the internet";
    public List<string> Dependencies => new();

    public Task<bool> IsInstalledAsync()
    {
        if (ProcessHelper.IsToolInstalled("ngrok"))
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
        var zipPath = Path.Combine(tempPath, ZipFileName);

        try
        {
            // Try winget first
            if (ProcessHelper.IsToolInstalled("winget"))
            {
                ConsoleHelper.WriteInfo($"Installing {Name} via winget...");
                var output = ProcessHelper.GetCommandOutput("winget",
                    "install --id=ngrok.ngrok -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                    return true;
                }
            }

            // Fallback to direct download and extract
            await DownloadManager.DownloadFileAsync(DownloadUrl, zipPath, Name, cancellationToken);

            var extractPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "ngrok");

            ConsoleHelper.WriteInfo($"Extracting {Name}...");
            
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath, true);

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            // Add to PATH for current session
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            if (!currentPath.Contains(extractPath))
            {
                Environment.SetEnvironmentVariable("PATH", $"{currentPath};{extractPath}", EnvironmentVariableTarget.User);
                ConsoleHelper.WriteInfo($"Added {extractPath} to user PATH");
            }

            ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to install {Name}: {ex.Message}");
            return false;
        }
    }
}