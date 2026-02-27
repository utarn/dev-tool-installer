namespace DevToolInstaller;

public interface IInstaller
{
    string Name { get; }
    DevelopmentCategory Category { get; }
    string Description { get; }
    List<string> Dependencies { get; }
    /// <summary>
    /// If true, this tool always runs during install and is not counted as "pending".
    /// Used for settings appliers (fonts, browser settings) that should be re-applied each time.
    /// </summary>
    bool AlwaysRun => false;
    Task<bool> IsInstalledAsync();
    Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default);
}