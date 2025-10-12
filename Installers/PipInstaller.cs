namespace DevToolInstaller.Installers;

public class PipInstaller : IInstaller
{
    public string Name => "Pip";
    public DevelopmentCategory Category => DevelopmentCategory.Python;
    public string Description => "Python package installer and manager";
    public List<string> Dependencies => new() { "Python" };

    public async Task<bool> IsInstalledAsync()
    {
        try
        {
            // Check if pip is available by running pip --version
            var result = ProcessHelper.GetCommandOutput("python", "-m pip --version");
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
            // First try to ensure pip is installed
            ConsoleHelper.WriteInfo("Ensuring pip is installed...");
            var ensureSuccess = ProcessHelper.ExecuteCommand("python", "-m ensurepip --default-pip");
            
            if (!ensureSuccess)
            {
                ConsoleHelper.WriteWarning("ensurepip failed, trying alternative installation method...");
            }

            // Upgrade pip to the latest version
            ConsoleHelper.WriteInfo("Upgrading pip to the latest version...");
            var upgradeSuccess = ProcessHelper.ExecuteCommand("python", "-m pip install --upgrade pip");

            if (upgradeSuccess)
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