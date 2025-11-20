namespace DevToolInstaller.Installers;

public class PostgreSQLInstaller : IInstaller
{
    public string Name => "PostgreSQL 18";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "PostgreSQL 18 database server for data storage and management";
    public List<string> Dependencies => new();

    public async Task<bool> IsInstalledAsync()
    {
        try
        {
            // Check if PostgreSQL is installed by looking for psql.exe
            if (await ProcessHelper.FindExecutableInPathAsync("psql.exe"))
            {
                ConsoleHelper.WriteWarning($"{Name} is already installed");
                return true;
            }

            // Check if PostgreSQL service is installed
            var output = await ProcessHelper.GetCommandOutput("powershell", 
                "Get-Service -Name postgres* -ErrorAction SilentlyContinue | Select-Object Name");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Installing PostgreSQL 18...");

        try
        {
            if (await ProcessHelper.FindExecutableInPathAsync("winget"))
            {
                progressReporter?.ReportStatus("Installing PostgreSQL 18 via winget...");
                progressReporter?.ReportProgress(20);
                var output = await ProcessHelper.GetCommandOutput("winget",
                    "install --id=PostgreSQL.PostgreSQL.18 -e --source=winget --accept-source-agreements --accept-package-agreements --force");

                if (output != null)
                {
                    progressReporter?.ReportStatus("PostgreSQL 18 installed successfully via winget!");
                    progressReporter?.ReportProgress(80);
                    
                    // Add PostgreSQL to PATH for current session
                    progressReporter?.ReportStatus("Configuring environment variables...");
                    var pgPath = @"C:\Program Files\PostgreSQL\18\bin";
                    var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                    
                    if (!currentPath.Contains(pgPath))
                    {
                        Environment.SetEnvironmentVariable("PATH", $"{currentPath};{pgPath}", EnvironmentVariableTarget.User);
                        progressReporter?.ReportSuccess($"Added {pgPath} to user PATH");
                    }
                    
                    progressReporter?.ReportProgress(100);
                    progressReporter?.ReportSuccess("PostgreSQL 18 installation completed successfully!");
                    return true;
                }
            }
            else
            {
                progressReporter?.ReportWarning("winget not found. Please install PostgreSQL 18 manually from the official website.");
                return false;
            }

            progressReporter?.ReportError("PostgreSQL 18 installation failed");
            return false;
        }
        catch (Exception ex)
        {
            progressReporter?.ReportError($"Failed to install PostgreSQL 18: {ex.Message}");
            return false;
        }
    }
}