namespace DevToolInstaller.Installers;

public class PoetryInstaller : IInstaller
{
    public string Name => "Poetry";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Python dependency management and packaging tool";
    public List<string> Dependencies => new() { "Python", "Pip" };

    public async Task<bool> IsInstalledAsync()
    {
        // 1. Check via FindExecutableInPathAsync (uses process PATH + registry PATH)
        if (await ProcessHelper.FindExecutableInPathAsync("poetry.exe"))
        {
            return true;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        // 2. Check Poetry official installer location: %APPDATA%\pypoetry\venv\Scripts\poetry.exe
        var poetryOfficialPath = Path.Combine(appData, "pypoetry", "venv", "Scripts", "poetry.exe");
        if (File.Exists(poetryOfficialPath))
        {
            return true;
        }

        // 3. Check %APPDATA%\Python\Scripts\poetry.exe (pip user install on some configs)
        var appDataPythonScripts = Path.Combine(appData, "Python", "Scripts", "poetry.exe");
        if (File.Exists(appDataPythonScripts))
        {
            return true;
        }

        // 4. Check Python3xx\Scripts directories under common bases
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
                    if (File.Exists(Path.Combine(pyDir, "Scripts", "poetry.exe")))
                    {
                        return true;
                    }
                }
            }
            catch { /* permission denied etc. */ }
        }

        // 5. Check user-level pip install: %APPDATA%\Python\Python3xx\Scripts\poetry.exe
        var appDataPython = Path.Combine(appData, "Python");
        if (Directory.Exists(appDataPython))
        {
            try
            {
                foreach (var pyDir in Directory.GetDirectories(appDataPython, "Python*"))
                {
                    if (File.Exists(Path.Combine(pyDir, "Scripts", "poetry.exe")))
                    {
                        return true;
                    }
                }
            }
            catch { /* permission denied etc. */ }
        }

        // 6. Check LocalAppData\Python\Python3xx\Scripts (another pip variant)
        var localAppDataPython = Path.Combine(localAppData, "Python");
        if (Directory.Exists(localAppDataPython))
        {
            try
            {
                foreach (var pyDir in Directory.GetDirectories(localAppDataPython, "Python*"))
                {
                    if (File.Exists(Path.Combine(pyDir, "Scripts", "poetry.exe")))
                    {
                        return true;
                    }
                }
            }
            catch { /* permission denied etc. */ }
        }

        // 7. Last resort: use 'where poetry' via cmd (catches PATH entries the process doesn't see)
        if (await ProcessHelper.FindExecutableWithWhereAsync("poetry"))
        {
            return true;
        }

        return false;
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Poetry...");

        try
        {
            // Ensure pip is up to date
            progressReporter?.ReportStatus("Updating pip...");
            progressReporter?.ReportProgress(10);
            await ProcessHelper.ExecuteCommand("python", "-m pip install --upgrade pip");
            
            progressReporter?.ReportStatus("Installing Poetry using pip...");
            progressReporter?.ReportProgress(30);
            var success = await ProcessHelper.ExecuteCommand("pip", "install poetry");

            if (success)
            {
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
}