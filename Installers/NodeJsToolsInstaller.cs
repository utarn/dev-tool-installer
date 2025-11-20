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
        var result = await ProcessHelper.GetCommandOutput("npm", "list -g --depth=0");
        if (string.IsNullOrWhiteSpace(result))
        {
            return false;
        }

        foreach (var tool in _tools.Keys)
        {
            if (!result.Contains(tool))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
    {
        ConsoleHelper.WriteInfo($"Installing {Name}...");

        try
        {
            var installedCount = 0;
            var totalCount = _tools.Count;

            foreach (var (toolName, description) in _tools)
            {
                ConsoleHelper.WriteInfo($"Checking {toolName}...");
                
                // Check if tool is already installed
                var result = await ProcessHelper.GetCommandOutput("npm", $"list -g {toolName}");
                if (!string.IsNullOrWhiteSpace(result) && !result.Contains("empty"))
                {
                    installedCount++;
                    continue;
                }

                ConsoleHelper.WriteInfo($"Installing {toolName}: {description}");
                var success = await ProcessHelper.ExecuteCommand("npm", $"install -g {toolName}");
                
                if (success)
                {
                    ConsoleHelper.WriteSuccess($"{toolName} installed successfully");
                    installedCount++;
                }
                else
                {
                    ConsoleHelper.WriteError($"Failed to install {toolName}");
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