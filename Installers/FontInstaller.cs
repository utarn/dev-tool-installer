using System.IO.Compression;
using System.Security.Principal;

namespace DevToolInstaller.Installers;

public class FontInstaller : IInstaller
{
    private const string CascadiaMonoDownloadUrl =
        "https://github.com/ryanoasis/nerd-fonts/releases/download/v3.4.0/CascadiaMono.zip";

    private static readonly string BundledThaiSarabunZip =
        Path.Combine(AppContext.BaseDirectory, "font", "THSARABUN_PSK.zip");

    public string Name => "Developer Fonts";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Download CascadiaMono Nerd Font and install bundled TH Sarabun into Windows Fonts";
    public List<string> Dependencies => new();
    public bool AlwaysRun => true;

    public Task<bool> IsInstalledAsync()
    {
        // Always allow running this installer so it can update/replace fonts when needed.
        return Task.FromResult(false);
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing fonts...");
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

        var extractedDir = Path.Combine(Path.GetTempPath(), $"devtoolinstaller-fonts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(extractedDir);

        try
        {
            // Step 1: Download CascadiaMono Nerd Font
            var cascadiaZipPath = Path.Combine(Path.GetTempPath(), "CascadiaMono.zip");
            progressReporter?.ReportStatus("Downloading CascadiaMono Nerd Font...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(CascadiaMonoDownloadUrl, cascadiaZipPath, "CascadiaMono Nerd Font", progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Extracting font archives...");
            progressReporter?.ReportProgress(30);

            ZipFile.ExtractToDirectory(cascadiaZipPath, extractedDir, overwriteFiles: true);

            // Clean up downloaded zip
            if (File.Exists(cascadiaZipPath))
                File.Delete(cascadiaZipPath);

            // Step 2: Extract bundled TH Sarabun
            if (File.Exists(BundledThaiSarabunZip))
            {
                progressReporter?.ReportStatus("Extracting TH Sarabun PSK fonts...");
                ZipFile.ExtractToDirectory(BundledThaiSarabunZip, extractedDir, overwriteFiles: true);
            }
            else
            {
                progressReporter?.ReportWarning($"Bundled TH Sarabun zip not found: {BundledThaiSarabunZip}. Skipping.");
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