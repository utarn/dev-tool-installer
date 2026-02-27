namespace DevToolInstaller.Installers;

public class UvInstaller : IInstaller
{
    public string Name => "uv";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Extremely fast Python package installer and resolver (Rust-based, replaces pip/virtualenv)";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("uv.exe") || ProcessHelper.IsToolInstalled("uv"))
        {
            return true;
        }
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing uv...");

        try
        {
            // Try winget first
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing uv via winget...");
                progressReporter?.ReportProgress(20);

                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=astral-sh.uv -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("uv installation completed successfully!");
                    return true;
                }
            }

            // Fallback: install via PowerShell script (official method)
            progressReporter?.ReportStatus("Installing uv via official installer...");
            progressReporter?.ReportProgress(30);

            var result = await ProcessHelper.GetCommandOutput("powershell",
                "-ExecutionPolicy Bypass -Command \"irm https://astral.sh/uv/install.ps1 | iex\"");

            if (result != null)
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("uv installation completed successfully!");
                return true;
            }

            progressReporter?.ReportError("uv installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install uv: {ex.Message}");
            return false;
        }
    }
}