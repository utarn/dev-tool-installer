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

    public static async Task<bool> FindExecutableInPathAsync(string executableName)
    {
        return await FindExecutablePathInPathAsync(executableName) != null;
    }

    public static async Task<string?> FindExecutablePathInPathAsync(string executableName)
    {
        if (string.IsNullOrWhiteSpace(executableName))
        {
            return null;
        }

        var searchPaths = new List<string?>();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appData = Environment.GetEnvironmentVariable("APPDATA");

            searchPaths.Add(programFiles);
            searchPaths.Add(programFilesX86);
            searchPaths.Add(Path.Combine(programFiles, "Git", "cmd"));
            searchPaths.Add(Path.Combine(programFiles, "nodejs"));
            searchPaths.Add(Path.Combine(localAppData, "Programs"));
            searchPaths.Add(Path.Combine(localAppData, "Microsoft", "WindowsApps"));
            if (!string.IsNullOrWhiteSpace(appData))
            {
                searchPaths.Add(Path.Combine(appData, "Local", "Programs"));
                searchPaths.Add(Path.Combine(appData, "npm"));
            }
        }
        
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(pathVariable))
        {
            searchPaths.AddRange(pathVariable.Split(Path.PathSeparator));
        }

        // Also check registry PATH (Machine + User) which may differ from process PATH
        var registryPath = GetRegistryPath();
        if (!string.IsNullOrWhiteSpace(registryPath))
        {
            foreach (var rp in registryPath.Split(Path.PathSeparator))
            {
                if (!string.IsNullOrWhiteSpace(rp) && !searchPaths.Contains(rp))
                {
                    searchPaths.Add(rp);
                }
            }
        }

        foreach (var path in searchPaths.Where(p => !string.IsNullOrWhiteSpace(p) && Directory.Exists(p)))
        {
            var fullPath = Path.Combine(path!, executableName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Path.HasExtension(executableName))
                {
                    var extensions = new[] { ".exe", ".cmd", ".bat", ".ps1" };
                    foreach (var ext in extensions)
                    {
                        var fileWithExtension = fullPath + ext;
                        if (File.Exists(fileWithExtension))
                        {
                            return fileWithExtension;
                        }
                    }
                }
            }

            if (File.Exists(fullPath))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Path.HasExtension(fullPath))
                {
                    continue;
                }
                return fullPath;
            }
        }

        return null;
    }

    public static bool ExecutePowerShellScript(string scriptPath, string arguments, bool waitForExit = true)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"& '{scriptPath}' {arguments}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
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
            ConsoleHelper.WriteError($"Failed to execute PowerShell script: {ex.Message}");
            return false;
        }
    }

    public static bool IsToolInstalled(string commandName)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c where {commandName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
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
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas",
                    WorkingDirectory = Path.GetTempPath()
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
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start /wait msiexec.exe /i \"{msiPath}\" {arguments}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
                }
            };

            process.Start();
            process.WaitForExit();
            
            // The exit code of 'start /wait' might not be the installer's exit code.
            // A zero exit code here usually means the command executed, but not necessarily that the installation succeeded.
            // For MSI installers, a more robust check involves checking the installation status afterwards, 
            // but for now, we'll assume success if the command doesn't throw and returns 0.
            // A common success exit code for msiexec is 0, but reboots can result in 3010.
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to execute MSI installer: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets PATH from the Windows Registry directly (Machine + User), bypassing cached process environment.
    /// This is important after installations that modify PATH — the process env may be stale.
    /// </summary>
    public static string GetRegistryPath()
    {
        var machinePath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
        var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
        return machinePath + Path.PathSeparator + userPath;
    }

    /// <summary>
    /// Uses 'where' command via cmd.exe to find an executable in the system PATH.
    /// This can find executables that the process PATH doesn't see.
    /// </summary>
    public static async Task<bool> FindExecutableWithWhereAsync(string executableName)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c where {executableName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output);
        }
        catch
        {
            return false;
        }
    }

    public static void RefreshEnvironmentVariables()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ConsoleHelper.WriteInfo("Refreshing environment variables...");

            // Refresh non-PATH environment variables from the machine level
            foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine))
            {
                if (env.Key != null && !string.Equals(env.Key.ToString(), "PATH", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.SetEnvironmentVariable(env.Key.ToString()!, env.Value?.ToString(), EnvironmentVariableTarget.Process);
                }
            }

            // Refresh non-PATH environment variables from the user level (user overrides machine)
            foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
            {
                if (env.Key != null && !string.Equals(env.Key.ToString(), "PATH", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.SetEnvironmentVariable(env.Key.ToString()!, env.Value?.ToString(), EnvironmentVariableTarget.Process);
                }
            }

            // Merge PATH: Machine PATH + User PATH (not overwrite)
            var machinePath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
            var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var mergedPath = string.IsNullOrEmpty(userPath) ? machinePath : machinePath + Path.PathSeparator + userPath;
            Environment.SetEnvironmentVariable("PATH", mergedPath, EnvironmentVariableTarget.Process);
            
            ConsoleHelper.WriteSuccess("Environment variables refreshed.");
        }
    }

    public static async Task<string?> GetCommandOutput(string command, string arguments)
    {
        try
        {
            var commandPath = await FindExecutablePathInPathAsync(command);

            if (commandPath == null)
            {
                return null; // Command not found
            }

            ProcessStartInfo startInfo;

            if (commandPath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"& '{commandPath}' {arguments}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
                };
            }
            else
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = commandPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
                };
            }
            
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            
            // Read stdout and stderr concurrently to prevent deadlocks
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            
            await Task.WhenAll(outputTask, errorTask);
            process.WaitForExit();

            var output = outputTask.Result;
            return process.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> ExecuteCommand(string command, string arguments)
    {
        try
        {
            var commandPath = await FindExecutablePathInPathAsync(command);

            if (commandPath == null)
            {
                ConsoleHelper.WriteError($"Command '{command}' not found in PATH.");
                return false; // Command not found
            }

            ProcessStartInfo startInfo;

            if (commandPath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"& '{commandPath}' {arguments}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
                };
            }
            else
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = commandPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
                };
            }

            using var process = new Process { StartInfo = startInfo };
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