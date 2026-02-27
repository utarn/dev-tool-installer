namespace DevToolInstaller.Installers;

public class RustDeskInstaller : IInstaller
{
    public string Name => "RustDesk";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Open-source remote desktop client with self-hosted server support";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("rustdesk.exe"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }

        if (ProcessHelper.IsToolInstalled("RustDesk"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }

        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing RustDesk...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing RustDesk via winget...");
                progressReporter?.ReportProgress(20);

                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=RustDesk.RustDesk -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("RustDesk installation completed successfully!");
                    return true;
                }
            }
            else
            {
                progressReporter?.ReportWarning("winget not found. Please install RustDesk manually from https://github.com/rustdesk/rustdesk/releases");
                return false;
            }

            progressReporter?.ReportError("RustDesk installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install RustDesk: {ex.Message}");
            return false;
        }
    }
}