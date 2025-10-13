namespace DevToolInstaller.Installers;

public class NpmInstaller : IInstaller
{
    public string Name => "NPM";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Node Package Manager - Package manager for JavaScript";
    public List<string> Dependencies => new() { "Node.js" };

    public async Task<bool> IsInstalledAsync()
    {
        if (!await ProcessHelper.FindExecutableInPathAsync("npm.cmd"))
        {
            return false;
        }
        
        try
        {
            // Check if npm is available by running npm --version
            var result = ProcessHelper.GetCommandOutput("npm", "--version");
            if (!string.IsNullOrWhiteSpace(result))
            {
                ConsoleHelper.WriteWarning($"{Name} is already installed");
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        try
        {
            // Update npm to the latest version
            ConsoleHelper.WriteInfo("Updating npm to the latest version...");
            var success = ProcessHelper.ExecuteCommand("npm", "install -g npm@latest");

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