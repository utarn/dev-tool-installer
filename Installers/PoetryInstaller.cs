using System.Runtime.InteropServices;

namespace DevToolInstaller.Installers;

public class PoetryInstaller : IInstaller
{
    public string Name => "Poetry";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Python dependency management and packaging tool";
    public List<string> Dependencies => new() { "Python", "Pip" };

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam,
        uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

    private static readonly IntPtr HWND_BROADCAST = new(0xFFFF);
    private const uint WM_SETTINGCHANGE = 0x001A;
    private const uint SMTO_ABORTIFHUNG = 0x0002;

    public async Task<bool> IsInstalledAsync()
    {
        // 1. Check via FindExecutableInPathAsync (uses process PATH + registry PATH)
        if (await ProcessHelper.FindExecutableInPathAsync("poetry.exe"))
        {
            return true;
        }

        // 2. Last resort: use 'where poetry' via cmd (catches PATH entries the process doesn't see)
        if (await ProcessHelper.FindExecutableWithWhereAsync("poetry"))
        {
            return true;
        }

        // If poetry.exe exists on disk but NOT in PATH, return false
        // so that reinstall will add the Scripts directory to PATH.
        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Poetry...");

        try
        {
            // Step 1: Check if poetry.exe already exists on disk (just not in PATH)
            progressReporter?.ReportStatus("Checking for existing Poetry installation...");
            progressReporter?.ReportProgress(10);
            var existingPoetryPath = await FindPoetryExecutable();

            if (existingPoetryPath != null)
            {
                // Poetry exists but not in PATH — just fix PATH
                progressReporter?.ReportStatus($"Poetry found at '{existingPoetryPath}' but not in PATH. Adding to PATH...");
                progressReporter?.ReportProgress(50);
                var scriptsDir = Path.GetDirectoryName(existingPoetryPath)!;
                AddToUserPath(scriptsDir);
                progressReporter?.ReportStatus($"Added '{scriptsDir}' to User PATH");

                progressReporter?.ReportStatus("Refreshing environment variables...");
                progressReporter?.ReportProgress(90);
                ProcessHelper.RefreshEnvironmentVariables();
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Poetry PATH configured successfully!");
                return true;
            }

            // Step 2: Poetry not found at all — install via pip
            // Ensure pip is up to date
            progressReporter?.ReportStatus("Updating pip...");
            progressReporter?.ReportProgress(20);
            await ProcessHelper.ExecuteCommand("python", "-m pip install --upgrade pip");
            
            progressReporter?.ReportStatus("Installing Poetry using pip...");
            progressReporter?.ReportProgress(40);
            var success = await ProcessHelper.ExecuteCommand("pip", "install poetry");

            if (success)
            {
                // Find where poetry.exe was installed and add Scripts dir to PATH
                progressReporter?.ReportStatus("Adding Python Scripts directory to PATH...");
                progressReporter?.ReportProgress(70);
                var poetryPath = await FindPoetryExecutable();
                if (poetryPath != null)
                {
                    var scriptsDir = Path.GetDirectoryName(poetryPath)!;
                    AddToUserPath(scriptsDir);
                    progressReporter?.ReportStatus($"Added '{scriptsDir}' to User PATH");
                }

                progressReporter?.ReportStatus("Refreshing environment variables...");
                progressReporter?.ReportProgress(90);
                ProcessHelper.RefreshEnvironmentVariables();
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Poetry installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Poetry installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Poetry: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Searches common locations for poetry.exe installed by pip.
    /// </summary>
    private static async Task<string?> FindPoetryExecutable()
    {
        // 1. Try to find via 'pip show poetry' → Location, then check Scripts sibling
        var pipShowOutput = await ProcessHelper.GetCommandOutput("pip", "show poetry");
        if (!string.IsNullOrWhiteSpace(pipShowOutput))
        {
            foreach (var line in pipShowOutput.Split('\n'))
            {
                if (line.StartsWith("Location:", StringComparison.OrdinalIgnoreCase))
                {
                    // Location points to site-packages dir, Scripts is a sibling
                    var sitePackages = line.Substring("Location:".Length).Trim();
                    var scriptsDir = Path.Combine(Path.GetDirectoryName(sitePackages) ?? sitePackages, "Scripts");
                    var candidate = Path.Combine(scriptsDir, "poetry.exe");
                    if (File.Exists(candidate)) return candidate;
                }
            }
        }

        // 2. Search known Python Scripts directories
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        var searchBases = new[]
        {
            Path.Combine(localAppData, "Programs", "Python"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python"),
            @"C:\Python",
        };

        foreach (var basePath in searchBases)
        {
            if (!Directory.Exists(basePath)) continue;
            try
            {
                foreach (var pyDir in Directory.GetDirectories(basePath, "Python*"))
                {
                    var candidate = Path.Combine(pyDir, "Scripts", "poetry.exe");
                    if (File.Exists(candidate)) return candidate;
                }
            }
            catch { /* permission denied etc. */ }
        }

        // 3. Check %APPDATA%\pypoetry\venv\Scripts (official Poetry installer location)
        var pypoetryVenv = Path.Combine(appData, "pypoetry", "venv", "Scripts", "poetry.exe");
        if (File.Exists(pypoetryVenv)) return pypoetryVenv;

        // 4. Check %APPDATA%\Python\Scripts and %APPDATA%\Python\Python3xx\Scripts
        var appDataPythonScripts = Path.Combine(appData, "Python", "Scripts", "poetry.exe");
        if (File.Exists(appDataPythonScripts)) return appDataPythonScripts;

        var appDataPython = Path.Combine(appData, "Python");
        if (Directory.Exists(appDataPython))
        {
            try
            {
                foreach (var pyDir in Directory.GetDirectories(appDataPython, "Python*"))
                {
                    var candidate = Path.Combine(pyDir, "Scripts", "poetry.exe");
                    if (File.Exists(candidate)) return candidate;
                }
            }
            catch { /* permission denied */ }
        }

        // 5. Check LocalAppData\Python\Python3xx\Scripts
        var localAppDataPython = Path.Combine(localAppData, "Python");
        if (Directory.Exists(localAppDataPython))
        {
            try
            {
                foreach (var pyDir in Directory.GetDirectories(localAppDataPython, "Python*"))
                {
                    var candidate = Path.Combine(pyDir, "Scripts", "poetry.exe");
                    if (File.Exists(candidate)) return candidate;
                }
            }
            catch { /* permission denied */ }
        }

        return null;
    }

    /// <summary>
    /// Adds a directory to the User PATH via registry and broadcasts WM_SETTINGCHANGE.
    /// </summary>
    private static void AddToUserPath(string directory)
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
        if (currentPath.Split(Path.PathSeparator).Any(p => p.Equals(directory, StringComparison.OrdinalIgnoreCase)))
        {
            return; // Already in PATH
        }

        var newPath = string.IsNullOrEmpty(currentPath) ? directory : currentPath + Path.PathSeparator + directory;
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);

        // Also update process PATH so the current session sees it immediately
        var processPath = Environment.GetEnvironmentVariable("PATH") ?? "";
        if (!processPath.Split(Path.PathSeparator).Any(p => p.Equals(directory, StringComparison.OrdinalIgnoreCase)))
        {
            Environment.SetEnvironmentVariable("PATH", processPath + Path.PathSeparator + directory);
        }

        // Broadcast WM_SETTINGCHANGE so other processes pick up the new PATH
        BroadcastSettingChange();
    }

    /// <summary>
    /// Broadcasts WM_SETTINGCHANGE to notify other processes of environment variable changes.
    /// </summary>
    private static void BroadcastSettingChange()
    {
        try
        {
            SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, UIntPtr.Zero, "Environment",
                SMTO_ABORTIFHUNG, 5000, out _);
        }
        catch
        {
            // Non-critical — best effort
        }
    }
}