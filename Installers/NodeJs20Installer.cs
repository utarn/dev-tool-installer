namespace DevToolInstaller.Installers;

public class NodeJs20Installer : IInstaller
{
    // Fallback version if API call fails
    private const string FallbackVersion = "20.19.6";
    
    private string _nodeVersion = string.Empty;

    public string Name => "Node.js 20";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node.js 20 JavaScript runtime with npm included";
    public List<string> Dependencies => new() { "NVM for Windows" };

    public NodeJs20Installer()
    {
        InitializeVersion();
    }

    private async void InitializeVersion()
    {
        var version = await VersionHelper.GetLatestNodeVersionAsync(20);
        _nodeVersion = version ?? FallbackVersion;
    }

    public async Task<bool> IsInstalledAsync()
    {
        var nvmList = await ProcessHelper.GetCommandOutput("nvm", "list");
        if (string.IsNullOrWhiteSpace(nvmList) || !nvmList.Contains(_nodeVersion.TrimStart('v')))
        {
            return false;
        }

        return await ProcessHelper.FindExecutableInPathAsync("node.exe") &&
               await ProcessHelper.GetCommandOutput("node", "--version") is not null;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        // Ensure version is initialized
        if (string.IsNullOrEmpty(_nodeVersion))
        {
            InitializeVersion();
            // Give it a moment to initialize
            await Task.Delay(100);
        }

        progressReporter?.ReportStatus("Installing Node.js 20 via nvm...");

        try
        {
            progressReporter?.ReportStatus($"Installing Node.js {_nodeVersion}...");
            progressReporter?.ReportProgress(40);
            var installSuccess = await ProcessHelper.ExecuteCommand("nvm", $"install {_nodeVersion}");

            if (!installSuccess)
            {
                progressReporter?.ReportError("Node.js 20 installation failed");
                return false;
            }

            progressReporter?.ReportStatus($"Activating Node.js {_nodeVersion}...");
            progressReporter?.ReportProgress(75);
            var useSuccess = await ProcessHelper.ExecuteCommand("nvm", $"use {_nodeVersion}");

            if (!useSuccess)
            {
                progressReporter?.ReportError("Failed to activate Node.js 20");
                return false;
            }

            progressReporter?.ReportStatus("Refreshing environment variables...");
            progressReporter?.ReportProgress(95);
            ProcessHelper.RefreshEnvironmentVariables();

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess($"Node.js 20 ({_nodeVersion}) installation completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Node.js 20: {ex.Message}");
            return false;
        }
    }
}