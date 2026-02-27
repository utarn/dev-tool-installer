namespace DevToolInstaller.Installers;

public class VisualCppBuildToolsInstaller : IInstaller
{
    private const string DownloadUrl = "https://aka.ms/vs/17/release/vs_buildtools.exe";
    private const string InstallerFileName = "vs_buildtools.exe";

    public string Name => "Visual C++ Build Tools";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Microsoft Visual C++ Build Tools for compiling Python packages";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        // Check if Visual C++ Build Tools are installed by looking for vcvarsall.bat
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        
        var possiblePaths = new[]
        {
            Path.Combine(programFiles, "Microsoft Visual Studio", "2022", "BuildTools", "VC", "Auxiliary", "Build", "vcvarsall.bat"),
            Path.Combine(programFilesX86, "Microsoft Visual Studio", "2022", "BuildTools", "VC", "Auxiliary", "Build", "vcvarsall.bat"),
            Path.Combine(programFiles, "Microsoft Visual Studio", "2019", "BuildTools", "VC", "Auxiliary", "Build", "vcvarsall.bat"),
            Path.Combine(programFilesX86, "Microsoft Visual Studio", "2019", "BuildTools", "VC", "Auxiliary", "Build", "vcvarsall.bat")
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
        progressReporter?.ReportStatus("Installing Visual C++ Build Tools...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            progressReporter?.ReportStatus("Downloading Visual C++ Build Tools installer...");
            progressReporter?.ReportProgress(10);
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, progressReporter, cancellationToken);

            progressReporter?.ReportStatus("Running Visual C++ Build Tools installer...");
            progressReporter?.ReportProgress(30);
            // Install C++ build tools workload with default components
            var arguments = "--quiet --wait --add Microsoft.VisualStudio.Workload.VCTools --includeRecommended";
            var success = ProcessHelper.ExecuteInstaller(installerPath, arguments);

            progressReporter?.ReportStatus("Cleaning up...");
            progressReporter?.ReportProgress(90);
            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Visual C++ Build Tools installation completed successfully!");
                return true;
            }
            else
            {
                progressReporter?.ReportError("Visual C++ Build Tools installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Visual C++ Build Tools: {ex.Message}");
            return false;
        }
    }
}