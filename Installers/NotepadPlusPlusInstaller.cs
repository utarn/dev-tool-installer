namespace DevToolInstaller.Installers;

public class NotepadPlusPlusInstaller : IInstaller
{
    public string Name => "Notepad++";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Free source code editor and Notepad replacement with syntax highlighting";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        // Check for notepad++ in PATH
        if (await ProcessHelper.FindExecutableInPathAsync("notepad++.exe"))
        {
            return true;
        }

        // Check common install locations
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        var possiblePaths = new[]
        {
            Path.Combine(programFiles, "Notepad++", "notepad++.exe"),
            Path.Combine(programFilesX86, "Notepad++", "notepad++.exe"),
        };

        foreach (var path in possiblePaths)
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
        progressReporter?.ReportStatus("Installing Notepad++...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing Notepad++ via winget...");
                progressReporter?.ReportProgress(20);

                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=Notepad++.Notepad++ -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("Notepad++ installation completed successfully!");
                    return true;
                }
            }
            else
            {
                progressReporter?.ReportWarning("winget not found. Please install Notepad++ manually.");
                return false;
            }

            progressReporter?.ReportError("Notepad++ installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Notepad++: {ex.Message}");
            return false;
        }
    }
}