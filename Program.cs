using DevToolInstaller;
using DevToolInstaller.Installers;

ConsoleHelper.WriteHeader("========================================");
ConsoleHelper.WriteHeader("Development Environment Setup");
ConsoleHelper.WriteHeader("========================================");
Console.WriteLine();

// Check if running as administrator
if (!ProcessHelper.IsAdministrator())
{
    ConsoleHelper.WriteWarning("This application requires Administrator privileges to install development tools.");
    ConsoleHelper.WriteInfo("Requesting Administrator privileges...");
    ProcessHelper.RestartAsAdministrator();
    return 0; // This line won't be reached due to Environment.Exit in RestartAsAdministrator
}

// Define all installers
var installers = new List<IInstaller>
{
    new DotNetSdkInstaller(),
    new VSCodeInstaller(),
    new GitInstaller(),
    new WindowsTerminalInstaller(),
    new PowerShell7Installer(),
    new DockerDesktopInstaller(),
    new NgrokInstaller()
};

var successCount = 0;
var failedCount = 0;
var skippedCount = 0;

using var cts = new CancellationTokenSource();

// Handle Ctrl+C gracefully
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    ConsoleHelper.WriteWarning("\nCancellation requested. Stopping installations...");
    cts.Cancel();
};

// Install each tool
foreach (var installer in installers)
{
    if (cts.Token.IsCancellationRequested)
    {
        ConsoleHelper.WriteWarning("Installation cancelled by user.");
        break;
    }

    try
    {
        Console.WriteLine();
        ConsoleHelper.WriteHeader($"Processing: {installer.Name}");
        ConsoleHelper.WriteHeader("----------------------------------------");

        // Check if already installed
        if (await installer.IsInstalledAsync())
        {
            skippedCount++;
            continue;
        }

        // Install the tool
        var success = await installer.InstallAsync(cts.Token);
        
        if (success)
        {
            successCount++;
        }
        else
        {
            failedCount++;
        }
    }
    catch (OperationCanceledException)
    {
        ConsoleHelper.WriteWarning($"Installation of {installer.Name} was cancelled.");
        break;
    }
    catch (Exception ex)
    {
        ConsoleHelper.WriteError($"Unexpected error installing {installer.Name}: {ex.Message}");
        failedCount++;
    }
}

// Summary
Console.WriteLine();
ConsoleHelper.WriteHeader("========================================");
ConsoleHelper.WriteHeader("Installation Summary");
ConsoleHelper.WriteHeader("========================================");
ConsoleHelper.WriteSuccess($"Successfully installed: {successCount}");
if (skippedCount > 0)
{
    ConsoleHelper.WriteWarning($"Already installed (skipped): {skippedCount}");
}
if (failedCount > 0)
{
    ConsoleHelper.WriteError($"Failed installations: {failedCount}");
}

Console.WriteLine();
if (successCount > 0)
{
    ConsoleHelper.WriteInfo("Please restart your terminal to ensure all changes take effect.");
    ConsoleHelper.WriteInfo("You may need to restart your computer for all changes to be fully applied.");
}

Console.WriteLine();
ConsoleHelper.WriteHeader("========================================");
ConsoleHelper.WriteHeader("Setup Complete!");
ConsoleHelper.WriteHeader("========================================");

return failedCount > 0 ? 1 : 0;