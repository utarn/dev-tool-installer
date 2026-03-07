namespace DevToolInstaller.Installers;

public class NodeJs24Installer : IInstaller
{
    // Fallback version if API call fails
    private const string FallbackVersion = "24.0.0";
    
    private string _downloadUrl = string.Empty;
    private string _installerFileName = string.Empty;
    private string _nodeVersion = string.Empty;

    public string Name => "Node.js 24";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js 24 JavaScript runtime with npm included (Latest Current Release)";
    public List<string> Dependencies => new();

    public NodeJs24Installer()
    {
        InitializeVersion();
    }

    private async void InitializeVersion()
    {
        var version = await VersionHelper.GetLatestNodeVersionAsync(24);
        _nodeVersion = version ?? FallbackVersion;
        var cleanVersion = _nodeVersion.TrimStart('v');
        _downloadUrl = $"https://nodejs.org/dist/{_nodeVersion}/node-{_nodeVersion}-x64.msi";
        _installerFileName = $"nodejs24-installer-{cleanVersion}.msi";
    }

    public async Task<bool> IsInstalledAsync()
    {
        return await ProcessHelper.FindExecutableInPathAsync("node.exe") &&
               await ProcessHelper.FindExecutableInPathAsync("npm.cmd");
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // Ensure version is initialized
        if (string.IsNullOrEmpty(_downloadUrl))
        {
            InitializeVersion();
            await Task.Delay(100);
        }

        progressReporter?.ReportStatus("Installing Node.js 24...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, _installerFileName);

        try
        {
            progressReporter?.ReportStatus($"Downloading Node.js 24 {_nodeVersion}...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(_downloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running Node.js 24 installer...");
            progressReporter?.ReportProgress(50);
            var success = ProcessHelper.ExecuteMsiInstaller(installerPath, "/quiet /norestart ADDLOCAL=ALL");

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
                progressReporter?.ReportSuccess($"Node.js 24 ({_nodeVersion}) installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Node.js 24 installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Node.js 24: {ex.Message}");
            return false;
        }
    }
}
