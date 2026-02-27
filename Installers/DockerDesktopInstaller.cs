using System.Text.Json;

namespace DevToolInstaller.Installers;

public class DockerDesktopInstaller : IInstaller
{
    private const string DownloadUrl = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe";
    private const string InstallerFileName = "DockerDesktopInstaller.exe";

    public string Name => "Docker Desktop";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Container platform for developing, shipping, and running applications";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("docker.exe") || ProcessHelper.IsToolInstalled("docker"))
        {
            return true;
        }
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Docker Desktop...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            // Try winget first
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing Docker Desktop via winget...");
                progressReporter?.ReportProgress(20);
                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=Docker.DockerDesktop -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportStatus("Configuring Docker Desktop...");
                    progressReporter?.ReportProgress(70);
                    await ConfigureDockerAsync(progressReporter);
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("Docker Desktop installation completed successfully!");
                    return true;
                }
            }

            // Fallback to direct download
            progressReporter?.ReportStatus("Downloading Docker Desktop installer...");
            progressReporter?.ReportProgress(30);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running Docker Desktop installer...");
            progressReporter?.ReportProgress(60);
            var success = ProcessHelper.ExecuteInstaller(installerPath, "install --quiet");

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(70);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                progressReporter?.ReportStatus("Configuring Docker Desktop...");
                progressReporter?.ReportProgress(80);
                await ConfigureDockerAsync(progressReporter);
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Docker Desktop installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Docker Desktop installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Docker Desktop: {ex.Message}");
            return false;
        }
    }

    private async Task ConfigureDockerAsync(IProgressReporter? progressReporter = null)
    {
        try
        {
            // Configure Docker Desktop settings
            progressReporter?.ReportStatus("Configuring Docker Desktop settings...");
            
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Docker Desktop", "settings.json");

            if (File.Exists(settingsPath))
            {
                var jsonString = await File.ReadAllTextAsync(settingsPath);
                var settings = JsonSerializer.Deserialize(jsonString, DockerSettingsContext.Default.DictionaryStringObject) ?? new();

                // Set memory to 2GB
                settings["memoryMiB"] = 2048;
                
                // Set CPUs to all available
                settings["cpus"] = Environment.ProcessorCount;
                
                // Set swap to 1GB
                settings["swapMiB"] = 1024;

                var updatedJson = JsonSerializer.Serialize(settings, DockerSettingsContext.Default.DictionaryStringObject);
                await File.WriteAllTextAsync(settingsPath, updatedJson);
                
                progressReporter?.ReportSuccess("Docker Desktop settings configured.");
            }

            // Enable Docker Desktop to start on Windows boot
            var dockerPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Docker", "Docker", "Docker Desktop.exe");

            if (File.Exists(dockerPath))
            {
                await ProcessHelper.GetCommandOutput("reg",
                    $"add \"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /v \"Docker Desktop\" /t REG_SZ /d \"\\\"{dockerPath}\\\"\" /f");
                progressReporter?.ReportSuccess("Docker Desktop will start on boot.");
            }

            // Start Docker Desktop
            progressReporter?.ReportStatus("Starting Docker Desktop...");
            if (File.Exists(dockerPath))
            {
                ProcessHelper.ExecuteInstaller(dockerPath, "--minimized", false);
                progressReporter?.ReportSuccess("Docker Desktop started (minimized).");
            }

            // Wait for Docker to be ready
            progressReporter?.ReportStatus("Waiting for Docker to be ready...");
            var maxRetries = 10;
            var dockerReady = false;
            
            for (int i = 0; i < maxRetries && !dockerReady; i++)
            {
                await Task.Delay(5000);
                var output = await ProcessHelper.GetCommandOutput("docker", "ps");
                if (output != null)
                {
                    dockerReady = true;
                }
            }

            if (dockerReady)
            {
                // Pull pgvector image
                progressReporter?.ReportStatus("Pulling pgvector/pgvector:pg17 image...");
                await ProcessHelper.GetCommandOutput("docker", "pull pgvector/pgvector:pg17");
                progressReporter?.ReportSuccess("Image pgvector/pgvector:pg17 pulled successfully.");
            }
            else
            {
                progressReporter?.ReportWarning("Docker did not start within expected time. Skipping image pull.");
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportWarning($"Failed to configure Docker Desktop: {ex.Message}");
        }
    }
}