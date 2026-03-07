using System.Diagnostics;

namespace DevToolInstaller.Installers;

public class ThaiFontInstaller : IInstaller
{
    // Local path to font-installer project (for development)
    private const string FontInstallerLocalPath = @"C:\Users\utarn\projects\font-installer\FontInstaller\FontInstaller.Console\bin\Release\net8.0\win-x64\publish\FontInstaller.Console.exe";
    
    // GitHub releases URL for font-installer (for production)
    private const string FontInstallerGitHubRepo = "utarn/font-installer";

    public string Name => "Thai Fonts";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Install Thai fonts (TH Sarabun and other Thai font families) using the font-installer tool";
    public List<string> Dependencies => new();
    public bool AlwaysRun => false;

    public async Task<bool> IsInstalledAsync()
    {
        // Check if Thai fonts are installed by looking for common Thai font names in Windows Fonts
        var windowsFontsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
        
        // Check for common Thai font files
        var thaiFontPatterns = new[] { "THSarabun", "TH Sarabun", "THMali", "Kanit", "Prompt" };
        
        foreach (var pattern in thaiFontPatterns)
        {
            var matchingFonts = Directory.GetFiles(windowsFontsDir, $"*{pattern}*", SearchOption.TopDirectoryOnly);
            if (matchingFonts.Length > 0)
            {
                return true;
            }
        }
        
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Thai Fonts...");

        if (!OperatingSystem.IsWindows())
        {
            progressReporter?.ReportWarning("Thai font installation is only supported on Windows.");
            return false;
        }

        try
        {
            string fontInstallerPath = await FindOrDownloadFontInstallerAsync(progressReporter, cancellationToken);
            
            if (string.IsNullOrEmpty(fontInstallerPath) || !File.Exists(fontInstallerPath))
            {
                progressReporter?.ReportError("Font installer executable not found.");
                return false;
            }

            progressReporter?.ReportStatus("Running Thai font installer...");
            progressReporter?.ReportProgress(50);

            // Run the font installer with --embedded flag to install embedded fonts
            var startInfo = new ProcessStartInfo
            {
                FileName = fontInstallerPath,
                Arguments = "--embedded",
                UseShellExecute = true,
                Verb = "runas", // Run as administrator
                CreateNoWindow = false
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            
            // Wait for the process to exit (with timeout)
            var timeout = TimeSpan.FromMinutes(5);
            var exited = await Task.Run(() => process.WaitForExit((int)timeout.TotalMilliseconds), cancellationToken);
            
            if (!exited)
            {
                progressReporter?.ReportWarning("Font installer is still running. Installation may continue in the background.");
            }

            progressReporter?.ReportProgress(90);
            
            // Give the system a moment to register fonts
            await Task.Delay(2000, cancellationToken);

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess("Thai font installation completed successfully!");
            return true;
        }
        catch (OperationCanceledException)
        {
            progressReporter?.ReportWarning("Thai font installation was canceled.");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Thai fonts: {ex.Message}");
            return false;
        }
    }

    private async Task<string> FindOrDownloadFontInstallerAsync(IProgressReporter? progressReporter, CancellationToken cancellationToken)
    {
        // Strategy 1: Check local development path first
        if (File.Exists(FontInstallerLocalPath))
        {
            progressReporter?.ReportStatus("Found font-installer in local development path.");
            return FontInstallerLocalPath;
        }

        // Strategy 2: Check if font-installer is in PATH
        var pathFontInstaller = await ProcessHelper.FindExecutablePathInPathAsync("FontInstaller.Console.exe");
        if (!string.IsNullOrEmpty(pathFontInstaller))
        {
            progressReporter?.ReportStatus("Found font-installer in PATH.");
            return pathFontInstaller;
        }

        // Strategy 3: Download from GitHub releases
        progressReporter?.ReportStatus("Downloading font-installer from GitHub releases...");
        progressReporter?.ReportProgress(10);

        try
        {
            var downloadUrl = await VersionHelper.GetThaiFontInstallerUrlAsync();
            if (string.IsNullOrEmpty(downloadUrl))
            {
                // Fallback: construct URL manually
                downloadUrl = $"https://github.com/{FontInstallerGitHubRepo}/releases/latest/download/FontInstaller.Console.exe";
            }

            var tempPath = Path.GetTempPath();
            var installerPath = Path.Combine(tempPath, "FontInstaller.Console.exe");

            progressReporter?.ReportStatus($"Downloading from: {downloadUrl}");
            await DownloadManager.DownloadFileAsync(downloadUrl, installerPath, "Font Installer", progressReporter, cancellationToken);

            if (File.Exists(installerPath))
            {
                progressReporter?.ReportStatus("Font installer downloaded successfully.");
                return installerPath;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportWarning($"Failed to download font-installer: {ex.Message}");
        }

        // Strategy 4: Check for alternative local paths (macOS/Linux development)
        var alternativePaths = new[]
        {
            "/Users/utarn/projects/font-installer/FontInstaller/FontInstaller.Console/dist/FontInstaller.Console",
            "/Users/utarn/projects/font-installer/FontInstaller/FontInstaller.Console/bin/Release/net8.0/osx-arm64/publish/FontInstaller.Console",
            Path.Combine(Path.GetTempPath(), "font-installer", "FontInstaller.Console.exe")
        };

        foreach (var altPath in alternativePaths)
        {
            if (File.Exists(altPath))
            {
                progressReporter?.ReportStatus($"Found font-installer at: {altPath}");
                return altPath;
            }
        }

        return string.Empty;
    }
}
