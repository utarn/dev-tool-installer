namespace DevToolInstaller.Installers;

public class WindowsTerminalInstaller : IInstaller
{
    public string Name => "Windows Terminal";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Modern terminal application for Windows with tabs, panes, and Unicode support";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        try
        {
            var output = await ProcessHelper.GetCommandOutput("powershell",
                "-Command \"Get-AppxPackage -Name 'Microsoft.WindowsTerminal' | Select-Object -ExpandProperty Version\"");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                return true;
            }
        }
        catch
        {
            // Continue with installation
        }
        
        // Secondary check: check for wt.exe in PATH
        if (await ProcessHelper.FindExecutableInPathAsync("wt.exe"))
        {
            return true;
        }
        
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Windows Terminal...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing Windows Terminal via winget...");
                progressReporter?.ReportProgress(30);
                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=Microsoft.WindowsTerminal -e --source=winget --accept-source-agreements --accept-package-agreements --force");
                 
                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("Windows Terminal installation completed successfully!");
                    return true;
                }
            }
            else
            {
                progressReporter?.ReportWarning("winget not found. Please install Windows Terminal manually from Microsoft Store.");
                return false;
            }

            progressReporter?.ReportError("Windows Terminal installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Windows Terminal: {ex.Message}");
            return false;
        }
    }
}