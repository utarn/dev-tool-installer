namespace DevToolInstaller.Installers;

public class NodeJsToolsInstaller : IInstaller
{
    private static readonly Dictionary<string, string> _tools = new()
    {
        { "nodemon", "Tool for automatically restarting Node.js applications on file changes" },
        { "express-generator", "Express.js application generator for scaffolding" },
        { "typescript", "TypeScript compiler for JavaScript development" },
        { "ts-node", "TypeScript execution environment for Node.js" }
    };

    public string Name => "Node.js Development Tools";
    public DevelopmentCategory Category => DevelopmentCategory.NodeJS;
    public string Description => "Common Node.js development tools including nodemon, express-generator, typescript, and ts-node";
    public List<string> Dependencies => new() { "Node.js" };

    public async Task<bool> IsInstalledAsync()
    {
        var allInstalled = true;
        
        foreach (var tool in _tools.Keys)
        {
            var result = ProcessHelper.GetCommandOutput("npm", $"list -g {tool}");
            if (string.IsNullOrWhiteSpace(result) || result.Contains("empty"))
            {
                allInstalled = false;
                break;
            }
        }

        if (allInstalled)
        {
            ConsoleHelper.WriteWarning($"{Name} are already installed");
            return true;
        }
        
        return false;
    }

    public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        try
        {
            var allSuccess = true;
            var installedCount = 0;
            var totalCount = _tools.Count;

            foreach (var (toolName, description) in _tools)
            {
                ConsoleHelper.WriteInfo($"Checking {toolName}...");
                
                // Check if tool is already installed
                var result = ProcessHelper.GetCommandOutput("npm", $"list -g {toolName}");
                if (!string.IsNullOrWhiteSpace(result) && !result.Contains("empty"))
                {
                    ConsoleHelper.WriteWarning($"{toolName} is already installed");
                    installedCount++;
                    continue;
                }

                ConsoleHelper.WriteInfo($"Installing {toolName}: {description}");
                var success = ProcessHelper.ExecuteCommand("npm", $"install -g {toolName}");
                
                if (success)
                {
                    ConsoleHelper.WriteSuccess($"{toolName} installed successfully");
                    installedCount++;
                }
                else
                {
                    ConsoleHelper.WriteError($"Failed to install {toolName}");
                    allSuccess = false;
                }
            }

            if (installedCount == totalCount)
            {
                ConsoleHelper.WriteSuccess($"{Name} installation completed successfully!");
                return true;
            }
            else if (installedCount > 0)
            {
                ConsoleHelper.WriteWarning($"{Name} partially installed: {installedCount}/{totalCount} tools installed");
                return false;
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