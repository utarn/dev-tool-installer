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
    public List<string> Dependencies => new() { "Node.js 20" };

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

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing Node.js Development Tools...");

        try
        {
            var installedCount = 0;
            var totalCount = _tools.Count;

            foreach (var (toolName, description) in _tools)
            {
                progressReporter?.ReportStatus($"Checking {toolName}...");
                
                // Check if tool is already installed
                var result = await ProcessHelper.GetCommandOutput("npm", $"list -g {toolName}");
                if (!string.IsNullOrWhiteSpace(result) && !result.Contains("empty"))
                {
                    installedCount++;
                    continue;
                }

                var progress = 10 + (installedCount * 80 / totalCount);
                progressReporter?.ReportStatus($"Installing {toolName}: {description}");
                progressReporter?.ReportProgress(progress);
                var success = await ProcessHelper.ExecuteCommand("npm", $"install -g {toolName}");
                
                if (success)
                {
                    progressReporter?.ReportSuccess($"{toolName} installed successfully");
                    installedCount++;
                }
                else
                {
                    progressReporter?.ReportError($"Failed to install {toolName}");
                }
            }

            if (installedCount == totalCount)
            {
                progressReporter?.ReportProgress(100);
                progressReporter?.ReportSuccess("Node.js Development Tools installation completed successfully!");
                return true;
            }
            else if (installedCount > 0)
            {
                progressReporter?.ReportWarning($"Node.js Development Tools partially installed: {installedCount}/{totalCount} tools installed");
                return false;
            }
            else
            {
                progressReporter?.ReportError("Node.js Development Tools installation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install Node.js Development Tools: {ex.Message}");
            return false;
        }
    }
}