namespace DevToolInstaller.Installers;

public class VisualCppBuildToolsInstaller : IInstaller
{
    private const string DownloadUrl = "https://aka.ms/vs/17/release/vs_buildtools.exe";
    private const string InstallerFileName = "vs_buildtools.exe";

    public string Name => "Visual C++ Build Tools";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Microsoft Visual C++ Build Tools for compiling Python packages";
    public List<string> Dependencies => new();

    public Task<bool> IsInstalledAsync()
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
                ConsoleHelper.WriteWarning($"{Name} is already installed");
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        var tempPath = Path.GetTempPath();
        var installerPath = Path.Combine(tempPath, InstallerFileName);

        try
        {
            await DownloadManager.DownloadFileAsync(DownloadUrl, installerPath, Name, cancellationToken);

            ConsoleHelper.WriteInfo($"Running {Name} installer...");
            // Install the C++ build tools workload with default components
            var arguments = "--quiet --wait --add Microsoft.VisualStudio.Workload.VCTools --includeRecommended";
            var success = ProcessHelper.ExecuteInstaller(installerPath, arguments);

            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            if (success)
            {
                ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                return true;
            }
            else
            {
                ConsoleHelper.WriteError($"{Name} installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to install {Name}: {ex.Message}");
            return false;
        }
    }
}