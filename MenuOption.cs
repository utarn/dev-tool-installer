namespace DevToolInstaller;

public class MenuOption
{
    public string Text { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IInstaller? Installer { get; set; }
    public DevelopmentCategory? Category { get; set; }
    public bool IsInstalled { get; set; }
    
    public MenuOption(string text, string? description = null)
    {
        Text = text;
        Description = description;
    }
    
    public MenuOption(string text, IInstaller installer, bool isInstalled = false)
    {
        Text = text;
        Installer = installer;
        IsInstalled = isInstalled;
    }
    
    public MenuOption(string text, DevelopmentCategory category, string? description = null)
    {
        Text = text;
        Category = category;
        Description = description;
    }
}