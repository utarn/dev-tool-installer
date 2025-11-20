namespace DevToolInstaller.Installers;

public class FlowiseInstaller : IInstaller
{
    public string Name => "Flowise";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Flowise - Drag & drop UI to build your customized LLM flow";
    public List<string> Dependencies => new() { "Node.js", "npm" };

    public async Task<bool> IsInstalledAsync()
    {
        try
        {
            var output = await ProcessHelper.GetCommandOutput("flowise", "--version");
            return !string.IsNullOrWhiteSpace(output);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Flowise...");

        try
        {
            progressReporter?.ReportStatus("Checking npm availability...");
            progressReporter?.ReportProgress(10);
            
            var npmVersion = await ProcessHelper.GetCommandOutput("npm", "--version");
            if (string.IsNullOrWhiteSpace(npmVersion))
            {
                progressReporter?.ReportError("npm is not available. Please install Node.js first.");
                return false;
            }

            progressReporter?.ReportStatus("Installing Flowise globally via npm...");
            progressReporter?.ReportProgress(30);
            
            var installSuccess = await ProcessHelper.ExecuteCommand("npm", "install -g flowise");
            
            if (!installSuccess)
            {
                progressReporter?.ReportError("Failed to install Flowise");
                return false;
            }

            progressReporter?.ReportStatus("Verifying Flowise installation...");
            progressReporter?.ReportProgress(80);
            
            var flowiseVersion = await ProcessHelper.GetCommandOutput("flowise", "--version");
            if (!string.IsNullOrWhiteSpace(flowiseVersion))
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Flowise installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Flowise installation verification failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Flowise: {ex.Message}");
            return false;
        }
    }
}