namespace DevToolInstaller;

public interface IInstaller
{
    string Name { get; }
    DevelopmentCategory Category { get; }
    string Description { get; }
    List<string> Dependencies { get; }
    Task<bool> IsInstalledAsync();
    Task<bool> InstallAsync(CancellationToken cancellationToken = default);
}