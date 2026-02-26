using System.IO.Compression;
using System.Security.Principal;

namespace DevToolInstaller.Installers;

public class FontInstaller : IInstaller
{
    private static readonly string[] FontZipPaths =
    [
        Path.Combine(AppContext.BaseDirectory, "font", "CaskaydiaMonoNerdFontPropo-Regular.zip"),
        Path.Combine(AppContext.BaseDirectory, "font", "THSARABUN_PSK.zip")
    ];

    public string Name => "Developer Fonts";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Install bundled development fonts from zip files into Windows Fonts";
    public List<string> Dependencies => new();

    public Task<bool> IsInstalledAsync()
    {
        // Always allow running this installer so it can update/replace fonts when needed.
        return Task.FromResult(false);
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing bundled fonts...");
        progressReporter?.ReportProgress(5);

        if (!IsRunningAsAdministrator())
        {
            progressReporter?.ReportError("Installing fonts requires Administrator privileges.");
            return false;
        }

        if (!OperatingSystem.IsWindows())
        {
            progressReporter?.ReportWarning("Font installation is only supported on Windows.");
            return false;
        }

        var missingZips = FontZipPaths.Where(path => !File.Exists(path)).ToList();
        if (missingZips.Count > 0)
        {
            progressReporter?.ReportError($"Font zip file not found: {string.Join(", ", missingZips)}");
            return false;
        }

        var extractedDir = Path.Combine(Path.GetTempPath(), $"devtoolinstaller-fonts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(extractedDir);

        try
        {
            progressReporter?.ReportStatus("Extracting font archives...");
            progressReporter?.ReportProgress(20);

            foreach (var zipPath in FontZipPaths)
            {
                ZipFile.ExtractToDirectory(zipPath, extractedDir, overwriteFiles: true);
            }

            var fontFiles = Directory
                .EnumerateFiles(extractedDir, "*.*", SearchOption.AllDirectories)
                .Where(file =>
                    file.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (fontFiles.Count == 0)
            {
                progressReporter?.ReportError("No .ttf or .otf files found inside font zip archives.");
                return false;
            }

            var windowsFontsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "Fonts");

            if (!Directory.Exists(windowsFontsDir))
            {
                progressReporter?.ReportError("Windows Fonts directory not found.");
                return false;
            }

            progressReporter?.ReportStatus($"Installing {fontFiles.Count} font file(s)...");
            int copied = 0;

            foreach (var fontFile in fontFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var targetPath = Path.Combine(windowsFontsDir, Path.GetFileName(fontFile));

                // Explicitly overwrite existing font file with the same file name.
                File.Copy(fontFile, targetPath, overwrite: true);
                copied++;

                var progress = 20 + (int)(75.0 * copied / fontFiles.Count);
                progressReporter?.ReportProgress(progress);
            }

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess($"Font installation completed. {copied} file(s) copied to Windows Fonts.");
            return true;
        }
        catch (OperationCanceledException)
        {
            progressReporter?.ReportWarning("Font installation was canceled.");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install fonts: {ex.Message}");
            return false;
        }
        finally
        {
            try
            {
                if (Directory.Exists(extractedDir))
                {
                    Directory.Delete(extractedDir, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private static bool IsRunningAsAdministrator()
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}