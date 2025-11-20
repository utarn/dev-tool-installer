namespace DevToolInstaller.Installers;

public class WindowsTerminalInstaller : IInstaller
{
    public string Name => "Windows Terminal";
    public DevelopmentCategory Category => DevelopmentCategory.CSharp;
    public string Description => "Modern terminal application for Windows with tabs, panes, and Unicode support";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        try
        {
            var output = await ProcessHelper.GetCommandOutput("powershell",
                "-Command \"Get-AppxPackage -Name 'Microsoft.WindowsTerminal' | Select-Object -ExpandProperty Version\"");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                return true;
            }
        }
        catch
        {
            // Continue with installation
        }
        
        // Secondary check: check for wt.exe in PATH
        if (await ProcessHelper.FindExecutableInPathAsync("wt.exe"))
        {
            return true;
        }
        
        return false;
    }

    public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                ConsoleHelper.WriteInfo($"Installing {Name} via winget...");
                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=Microsoft.WindowsTerminal -e --source=winget --accept-source-agreements --accept-package-agreements --force");
                
                if (output != null)
                {
                    ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                    return true;
                }
            }
            else
            {
                ConsoleHelper.WriteWarning("winget not found. Please install Windows Terminal manually from the Microsoft Store.");
                return false;
            }

            ConsoleHelper.WriteError($"{Name} installation failed");
            return false;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to install {Name}: {ex.Message}");
            return false;
        }
    }
}