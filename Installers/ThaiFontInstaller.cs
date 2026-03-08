using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Microsoft.Win32;

namespace DevToolInstaller.Installers;

public class ThaiFontInstaller : IInstaller
{
    /// <summary>Directory containing embedded Thai font TTF files, relative to the executable.</summary>
    private const string ThaiFontDirectory = "font/thai";

    /// <summary>Representative font file used to check if Thai fonts are already installed.</summary>
    private const string RepresentativeFontFile = "THSarabunNew.ttf";

    /// <summary>Registry key where Windows stores installed font entries.</summary>
    private const string FontsRegistryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

    // Win32 APIs for immediate font registration (no reboot needed)
    [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", CharSet = CharSet.Unicode)]
    private static extern int AddFontResource(string lpFileName);

    [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW")]
    private static extern nint SendMessageTimeout(
        nint hWnd, uint msg, nint wParam, nint lParam,
        uint fuFlags, uint uTimeout, out nint lpdwResult);

    private const nint HWND_BROADCAST = 0xFFFF;
    private const uint WM_FONTCHANGE = 0x001D;
    private const uint SMTO_ABORTIFHUNG = 0x0002;

    public string Name => "Thai Fonts";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Install Thai fonts (TH Sarabun, Chakra Petch, KoHo, and other Thai font families)";
    public List<string> Dependencies => new();
    public bool AlwaysRun => false;

    public Task<bool> IsInstalledAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult(false);
        }

        var windowsFontsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "Fonts");

        var representativeFont = Path.Combine(windowsFontsDir, RepresentativeFontFile);
        return Task.FromResult(File.Exists(representativeFont));
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Thai Fonts...");
        progressReporter?.ReportProgress(5);

        if (!OperatingSystem.IsWindows())
        {
            progressReporter?.ReportWarning("Thai font installation is only supported on Windows.");
            return false;
        }

        if (!IsRunningAsAdministrator())
        {
            progressReporter?.ReportError("Installing fonts requires Administrator privileges.");
            return false;
        }

        try
        {
            // Locate the embedded font directory relative to the executable
            var fontSourceDir = Path.Combine(AppContext.BaseDirectory, ThaiFontDirectory);

            if (!Directory.Exists(fontSourceDir))
            {
                progressReporter?.ReportError($"Thai font directory not found: {fontSourceDir}");
                return false;
            }

            var fontFiles = Directory
                .EnumerateFiles(fontSourceDir, "*.ttf", SearchOption.TopDirectoryOnly)
                .ToList();

            if (fontFiles.Count == 0)
            {
                progressReporter?.ReportError("No .ttf files found in the Thai font directory.");
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

            progressReporter?.ReportStatus($"Installing {fontFiles.Count} Thai font file(s)...");
            progressReporter?.ReportProgress(10);
            int copied = 0;
            int skipped = 0;

            foreach (var fontFile in fontFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileName = Path.GetFileName(fontFile);
                var targetPath = Path.Combine(windowsFontsDir, fileName);

                // Skip if font already exists with the same file size
                if (File.Exists(targetPath))
                {
                    var sourceInfo = new FileInfo(fontFile);
                    var targetInfo = new FileInfo(targetPath);
                    if (sourceInfo.Length == targetInfo.Length)
                    {
                        skipped++;
                        var progress = 10 + (int)(80.0 * (copied + skipped) / fontFiles.Count);
                        progressReporter?.ReportProgress(progress);
                        continue;
                    }
                }

                // Copy font file to Windows Fonts directory
                File.Copy(fontFile, targetPath, overwrite: true);

                // Register font in registry so Windows recognizes the font name
                var fontName = Path.GetFileNameWithoutExtension(fileName);
                fontsKey?.SetValue(fontName + " (TrueType)", fileName, RegistryValueKind.String);

                // Notify GDI about the new font (makes it available immediately)
                AddFontResource(targetPath);

                copied++;

                var prog = 10 + (int)(80.0 * (copied + skipped) / fontFiles.Count);
                progressReporter?.ReportProgress(prog);
            }

            // Broadcast WM_FONTCHANGE so all applications pick up the new fonts
            progressReporter?.ReportStatus("Broadcasting font change notification...");
            SendMessageTimeout(HWND_BROADCAST, WM_FONTCHANGE, 0, 0, SMTO_ABORTIFHUNG, 5000, out _);

            progressReporter?.ReportProgress(100);

            var message = $"Thai font installation completed. {copied} file(s) installed, {skipped} file(s) skipped (already up to date).";
            progressReporter?.ReportSuccess(message);
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

    [SupportedOSPlatform("windows")]
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