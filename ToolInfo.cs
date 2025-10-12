namespace DevToolInstaller;

public class ToolInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DevelopmentCategory Category { get; set; }
    public bool IsInstalled { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public IInstaller Installer { get; set; } = null!;
}