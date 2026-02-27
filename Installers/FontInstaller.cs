using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace DevToolInstaller.Installers;

public partial class FontInstaller : IInstaller
{
    private const string CascadiaMonoDownloadUrl =
        "https://github.com/ryanoasis/nerd-fonts/releases/download/v3.4.0/CascadiaMono.zip";

    private static readonly string BundledThaiSarabunZip =
        Path.Combine(AppContext.BaseDirectory, "font", "THSARABUN_PSK.zip");

    /// <summary>Registry key where Windows stores installed font entries.</summary>
    private const string FontsRegistryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

    // Win32 APIs for immediate font registration (no reboot needed)
    [LibraryImport("gdi32.dll", EntryPoint = "AddFontResourceW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int AddFontResource(string lpFileName);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageTimeoutW")]
    private static partial nint SendMessageTimeout(
        nint hWnd, uint msg, nint wParam, nint lParam,
        uint fuFlags, uint uTimeout, out nint lpdwResult);

    private const nint HWND_BROADCAST = 0xFFFF;
    private const uint WM_FONTCHANGE = 0x001D;
    private const uint SMTO_ABORTIFHUNG = 0x0002;

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

            // Open registry key for font registration (HKLM — requires admin)
            using var fontsKey = Registry.LocalMachine.OpenSubKey(FontsRegistryKey, writable: true);
            if (fontsKey == null)
            {
                progressReporter?.ReportWarning("Could not open Fonts registry key. Fonts will be copied but may need a restart.");
            }

            progressReporter?.ReportStatus($"Installing {fontFiles.Count} font file(s)...");
            int copied = 0;

            foreach (var fontFile in fontFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileName = Path.GetFileName(fontFile);
                var targetPath = Path.Combine(windowsFontsDir, fileName);

                // Copy font file to Windows Fonts directory
                File.Copy(fontFile, targetPath, overwrite: true);

                // Register font in registry so Windows recognizes the font name
                // Registry value name = font display name, value = filename
                // For simplicity, use filename without extension + type suffix
                var fontName = Path.GetFileNameWithoutExtension(fileName);
                var fontType = fileName.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)
                    ? " (OpenType)" : " (TrueType)";
                fontsKey?.SetValue(fontName + fontType, fileName, RegistryValueKind.String);

                // Notify GDI about the new font (makes it available immediately)
                AddFontResource(targetPath);

                copied++;

                var progress = 20 + (int)(75.0 * copied / fontFiles.Count);
                progressReporter?.ReportProgress(progress);
            }

            // Broadcast WM_FONTCHANGE so all applications pick up the new fonts
            progressReporter?.ReportStatus("Broadcasting font change notification...");
            SendMessageTimeout(HWND_BROADCAST, WM_FONTCHANGE, 0, 0, SMTO_ABORTIFHUNG, 5000, out _);

            progressReporter?.ReportProgress(100);
            progressReporter?.ReportSuccess($"Font installation completed. {copied} file(s) installed and registered.");
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