using DevToolInstaller;
using System.Runtime.InteropServices;

// Check OS compatibility
var minWindowsVersion = new Version(10, 0, 10240);

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
    RuntimeInformation.OSArchitecture != Architecture.X64 ||
    Environment.OSVersion.Version < minWindowsVersion)
{
    ConsoleHelper.WriteError("This application requires Windows 10 (version 10.0.10240) or Windows 11, running on x64 architecture.");
    ConsoleHelper.WriteError($"Current OS: {Environment.OSVersion.VersionString}");
    ConsoleHelper.WriteError($"Current Architecture: {RuntimeInformation.OSArchitecture}");
    Console.WriteLine("Press any key to exit...");
    try
    {
        Console.ReadKey();
    }
    catch (InvalidOperationException)
    {
        // Handle cases where console input is not available
        System.Threading.Thread.Sleep(2000);
    }
    Environment.Exit(1);
}

// Auto-elevate to administrator (required for font installation, winget, etc.)
if (!ProcessHelper.IsAdministrator())
{
    ConsoleHelper.WriteWarning("Administrator privileges required. Restarting as administrator...");
    ProcessHelper.RestartAsAdministrator();
    return;
}

// Check and install winget if needed
await EnsureWingetInstalledAsync();

Console.Title = "DevToolInstaller";

using var menuSystem = new MenuSystem();
await menuSystem.RunAsync();

static async Task EnsureWingetInstalledAsync()
{
    if (await ProcessHelper.FindExecutableInPathAsync("winget"))
    {
        return; // winget is already installed
    }

    ConsoleHelper.WriteInfo("winget not found. Installing winget...");
    
    try
    {
        // Download and install winget
        var wingetUrl = "https://aka.ms/getwinget";
        var tempPath = Path.GetTempPath();
        var wingetInstaller = Path.Combine(tempPath, "winget.msixbundle");
        
        await DownloadManager.DownloadFileAsync(wingetUrl, wingetInstaller, "winget");
        
        var success = ProcessHelper.ExecuteInstaller(wingetInstaller, "/quiet");
        
        if (success)
        {
            ConsoleHelper.WriteSuccess("winget installed successfully!");
            
            // Refresh environment variables to make winget available
            ProcessHelper.RefreshEnvironmentVariables();
            
            // Wait a moment for the installation to complete
            await Task.Delay(2000);
        }
        else
        {
            ConsoleHelper.WriteWarning("Failed to install winget. Some installers may not work properly.");
        }
        
        // Clean up
        if (File.Exists(wingetInstaller))
        {
            File.Delete(wingetInstaller);
        }
    }
    catch (Exception ex)
    {
        ConsoleHelper.WriteError($"Failed to install winget: {ex.Message}");
        ConsoleHelper.WriteWarning("Some installers may not work properly without winget.");
    }
}