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
    Console.ReadKey();
    Environment.Exit(1);
}

Console.Title = "DevToolInstaller";

using var menuSystem = new MenuSystem();
await menuSystem.RunAsync();