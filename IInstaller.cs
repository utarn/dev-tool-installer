namespace DevToolInstaller;

public interface IInstaller
{
    string Name { get; }
    Task<bool> IsInstalledAsync();
    Task<bool> InstallAsync(CancellationToken cancellationToken = default);
}