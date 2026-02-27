namespace DevToolInstaller.Installers;

public class PostmanInstaller : IInstaller
{
    public string Name => "Postman";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "API platform for building, testing, and documenting APIs";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("Postman.exe"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }

        // Also check via winget list
        if (ProcessHelper.IsToolInstalled("Postman"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }

        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Postman...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing Postman via winget...");
                progressReporter?.ReportProgress(20);

                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=Postman.Postman -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("Postman installation completed successfully!");
                    return true;
                }
            }
            else
            {
                progressReporter?.ReportWarning("winget not found. Please install Postman manually.");
                return false;
            }

            progressReporter?.ReportError("Postman installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Postman: {ex.Message}");
            return false;
        }
    }
}