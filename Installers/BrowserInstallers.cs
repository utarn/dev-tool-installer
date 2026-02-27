namespace DevToolInstaller.Installers;

public abstract class WingetBrowserInstallerBase : IInstaller
{
    public abstract string Name { get; }
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public abstract string Description { get; }
    public List<string> Dependencies => new();

    protected abstract string PackageId { get; }
    protected abstract string[] ExecutableNames { get; }
    protected virtual string[] CommonInstallPaths => [];

    public async Task<bool> IsInstalledAsync()
    {
        foreach (var exe in ExecutableNames)
        {
            if (await ProcessHelper.FindExecutableInPathAsync(exe))
            {
                return true;
            }
        }

        // Check common install locations
        foreach (var path in CommonInstallPaths)
        {
            if (File.Exists(path))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus($"Installing {Name}...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus($"Installing {Name} via winget...");
                progressReporter?.ReportProgress(20);

                var output = await ProcessHelper.GetCommandOutput("winget",
                    $"install --id={PackageId} -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess($"{Name} installation completed successfully!");
                    return true;
                }
            }
            else
            {
                progressReporter?.ReportWarning("winget not found. Please install manually.");
                return false;
            }

            progressReporter?.ReportError($"{Name} installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install {Name}: {ex.Message}");
            return false;
        }
    }
}

public class ChromeInstaller : WingetBrowserInstallerBase
{
    public override string Name => "Google Chrome";
    public override string Description => "Fast, secure web browser from Google";
    protected override string PackageId => "Google.Chrome";
    protected override string[] ExecutableNames => ["chrome.exe"];
    protected override string[] CommonInstallPaths =>
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
    ];
}

public class FirefoxInstaller : WingetBrowserInstallerBase
{
    public override string Name => "Mozilla Firefox";
    public override string Description => "Privacy-focused open source web browser";
    protected override string PackageId => "Mozilla.Firefox";
    protected override string[] ExecutableNames => ["firefox.exe"];
    protected override string[] CommonInstallPaths =>
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Mozilla Firefox", "firefox.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Mozilla Firefox", "firefox.exe"),
    ];
}

public class BraveInstaller : WingetBrowserInstallerBase
{
    public override string Name => "Brave Browser";
    public override string Description => "Privacy-focused Chromium browser with built-in ad blocking";
    protected override string PackageId => "Brave.Brave";
    protected override string[] ExecutableNames => ["brave.exe"];
    protected override string[] CommonInstallPaths =>
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
    ];
}

public class OperaInstaller : WingetBrowserInstallerBase
{
    public override string Name => "Opera Browser";
    public override string Description => "Feature-rich web browser with built-in VPN and productivity tools";
    protected override string PackageId => "Opera.Opera";
    protected override string[] ExecutableNames => ["opera.exe", "launcher.exe"];
    protected override string[] CommonInstallPaths =>
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Opera", "opera.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Opera", "launcher.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Opera", "opera.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Opera", "opera.exe"),
    ];
}
