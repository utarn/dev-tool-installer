namespace DevToolInstaller.Installers;

public class PoetryInstaller : IInstaller
{
    public string Name => "Poetry";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Python dependency management and packaging tool";
    public List<string> Dependencies => new() { "Python", "Pip" };

    public async Task<bool> IsInstalledAsync()
    {
        if (await ProcessHelper.FindExecutableInPathAsync("poetry.exe"))
        {
            ConsoleHelper.WriteWarning($"{Name} is already installed");
            return true;
        }
        return false;
    }

    public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        try
        {
            // Ensure pip is up to date
            await ProcessHelper.ExecuteCommand("python", "-m pip install --upgrade pip");
            
            ConsoleHelper.WriteInfo($"Installing {Name} using pip...");
            var success = await ProcessHelper.ExecuteCommand("pip", "install poetry");

            if (success)
            {
                ProcessHelper.RefreshEnvironmentVariables();
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