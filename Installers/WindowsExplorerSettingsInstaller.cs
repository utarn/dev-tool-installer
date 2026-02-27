using Microsoft.Win32;

namespace DevToolInstaller.Installers;

public class WindowsExplorerSettingsInstaller : IInstaller
{
    public string Name => "Windows Explorer Settings";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Configure Windows Explorer: show hidden files, show file extensions";
    public List<string> Dependencies => new();

    private const string AdvancedKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

    public Task<bool> IsInstalledAsync()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AdvancedKey);
            if (key != null)
            {
                var hidden = key.GetValue("Hidden");
                var hideFileExt = key.GetValue("HideFileExt");

                // Hidden = 1 means show hidden files; HideFileExt = 0 means show extensions
                if (hidden is int h && h == 1 && hideFileExt is int e && e == 0)
                {
                    ConsoleHelper.WriteWarning($"{Name} already configured");
                    return Task.FromResult(true);
                }
            }
        }
        catch
        {
            // If we can't read registry, assume not configured
        }

        return Task.FromResult(false);
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Configuring Windows Explorer settings...");
        progressReporter?.ReportProgress(10);

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AdvancedKey, writable: true);
            if (key == null)
            {
                progressReporter?.ReportError("Could not open Explorer Advanced registry key.");
                return false;
            }

            // Show hidden files and folders
            progressReporter?.ReportStatus("Setting: Show hidden files and folders...");
            progressReporter?.ReportProgress(30);
            key.SetValue("Hidden", 1, RegistryValueKind.DWord);

            // Show file extensions
            progressReporter?.ReportStatus("Setting: Show file extensions...");
            progressReporter?.ReportProgress(60);
            key.SetValue("HideFileExt", 0, RegistryValueKind.DWord);

            // Refresh Explorer to apply changes immediately
            progressReporter?.ReportStatus("Refreshing Explorer...");
            progressReporter?.ReportProgress(80);
            await ProcessHelper.GetCommandOutput("cmd", "/c taskkill /f /im explorer.exe & start explorer.exe");

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess("Windows Explorer settings configured: show hidden files + show file extensions");
            return true;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to configure Windows Explorer settings: {ex.Message}");
            return false;
        }
    }
}