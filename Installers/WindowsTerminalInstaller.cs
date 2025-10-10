namespace DevToolInstaller.Installers;

public class WindowsTerminalInstaller : IInstaller
{
    public string Name => "Windows Terminal";

    public Task<bool> IsInstalledAsync()
    {
        try
        {
            var output = ProcessHelper.GetCommandOutput("powershell",
                "-Command \"Get-AppxPackage -Name 'Microsoft.WindowsTerminal' | Select-Object -ExpandProperty Version\"");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                ConsoleHelper.WriteWarning($"{Name} is already installed (version: {output.Trim()})");
                return Task.FromResult(true);
            }
        }
        catch
        {
            // Continue with installation
        }
        return Task.FromResult(false);
    }

    public Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        try
        {
            if (ProcessHelper.IsToolInstalled("winget"))
            {
                ConsoleHelper.WriteInfo($"Installing {Name} via winget...");
                var output = ProcessHelper.GetCommandOutput("winget",
                    "install --id=Microsoft.WindowsTerminal -e --source=winget --accept-source-agreements --accept-package-agreements --force");
                
                if (output != null)
                {
                    ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                    return Task.FromResult(true);
                }
            }
            else
            {
                ConsoleHelper.WriteWarning("winget not found. Please install Windows Terminal manually from the Microsoft Store.");
                return Task.FromResult(false);
            }

            ConsoleHelper.WriteError($"{Name} installation failed");
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to install {Name}: {ex.Message}");
            return Task.FromResult(false);
        }
    }
}