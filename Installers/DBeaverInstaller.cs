namespace DevToolInstaller.Installers;

public class DBeaverInstaller : IInstaller
{
    public string Name => "DBeaver";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Universal database tool for developers, database administrators, and analysts";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("dbeaver.exe"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }

        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing DBeaver...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing DBeaver via winget...");
                progressReporter?.ReportProgress(20);

                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=DBeaver.DBeaver -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("DBeaver installation completed successfully!");
                    return true;
                }
            }
            else
            {
                progressReporter?.ReportWarning("winget not found. Please install DBeaver manually.");
                return false;
            }

            progressReporter?.ReportError("DBeaver installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install DBeaver: {ex.Message}");
            return false;
        }
    }
}