using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevToolInstaller;

public static class ProcessHelper
{
    public static bool IsAdministrator()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;

        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    public static void RestartAsAdministrator()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Environment.ProcessPath,
                Verb = "runas"
            };

            Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to restart as administrator: {ex.Message}");
        }
    }

    public static Task<bool> FindExecutableInPathAsync(string executableName)
    {
        if (string.IsNullOrWhiteSpace(executableName))
        {
            return Task.FromResult(false);
        }

        // 1. Check common installation directories (Windows specific paths)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var commonPaths = new[]
            {
                programFiles,
                programFilesX86,
                Path.Combine(programFiles, "Git", "cmd"), // Common Git path
                Path.Combine(programFiles, "nodejs"), // Common Node.js path
                Path.Combine(localAppData, "Programs"),
                Path.Combine(appData, "Local", "Programs"),
                Path.Combine(appData, "npm"),
                Path.Combine(localAppData, "Microsoft", "WindowsApps") // Windows Store apps path
            };

            foreach (var path in commonPaths.Where(Directory.Exists))
            {
                var fullPath = Path.Combine(path, executableName);
                if (File.Exists(fullPath))
                {
                    return Task.FromResult(true);
                }
            }
        }
        
        // 2. Check all directories listed in the system's PATH environment variable
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(pathVariable))
        {
            var pathDirectories = pathVariable.Split(Path.PathSeparator);
            foreach (var directory in pathDirectories.Where(Directory.Exists))
            {
                var fullPath = Path.Combine(directory, executableName);
                
                // Check for common executable extensions on Windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var extensions = new[] { ".exe", ".cmd", ".bat" };
                    if (extensions.Any(ext => File.Exists(fullPath + ext)))
                    {
                        return Task.FromResult(true);
                    }
                }
                
                // Check for exact match (works for Linux/macOS and Windows if extensionless)
                if (File.Exists(fullPath))
                {
                    return Task.FromResult(true);
                }
            }
        }

        return Task.FromResult(false);
    }

    public static bool IsToolInstalled(string commandName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c where {commandName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output);
        }
        catch
        {
            return false;
        }
    }

    public static bool ExecuteInstaller(string installerPath, string arguments, bool waitForExit = true)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                }
            };

            process.Start();
            
            if (waitForExit)
            {
                process.WaitForExit();
                return process.ExitCode == 0;
            }

            return true;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to execute installer: {ex.Message}");
            return false;
        }
    }

    public static bool ExecuteMsiInstaller(string msiPath, string arguments = "/quiet /norestart")
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = $"/i \"{msiPath}\" {arguments}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to execute MSI installer: {ex.Message}");
            return false;
        }
    }

    public static string? GetCommandOutput(string command, string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }

    public static bool ExecuteCommand(string command, string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to execute command '{command} {arguments}': {ex.Message}");
            return false;
        }
    }
}