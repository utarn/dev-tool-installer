using DevToolInstaller.Installers;

namespace DevToolInstaller;

public static class ToolRegistry
{
    private static readonly List<IInstaller> _allInstallers = new()
    {
        // .NET SDKs
        new DotNetSdk10Installer(),
        new DotNetSdkInstaller(), // .NET 8
        
        // Cross-platform tools
        new VSCodeInstaller(),
        new GitInstaller(),
        new WindowsTerminalInstaller(),
        new PowerShell7Installer(),
        new DockerDesktopInstaller(),
        
        // Python ecosystem (uv first, then Python)
        new UvInstaller(),
        new PythonInstaller(),
        new PipInstaller(),
        new PoetryInstaller(),
        
        // Node.js ecosystem (NVM first, then Node versions)
        new NvmWindowsInstaller(),
        new NodeJs20Installer(),
        new NodeJs22Installer(),
        new NodeJs24Installer(),
        new NpmInstaller(),
        new NodeJsToolsInstaller(),
        
        // Development tools
        new VisualCppBuildToolsInstaller(),
        new OhMyPoshInstaller(),
        
        // Fonts (CascadiaMono and Thai fonts separately)
        new FontInstaller(),
        new ThaiFontInstaller(),
        
        // Other applications
        new NotepadPlusPlusInstaller(),
        new PostmanInstaller(),
        new RustDeskInstaller(),
        new WireGuardInstaller(),
        new ChromeInstaller(),
        new BraveInstaller(),
        new FirefoxInstaller(),
        new OperaInstaller(),
        new WindowsExplorerSettingsInstaller(),
        new BrowserSettingsInstaller(),
        new WslConfigInstaller()
    };

    public static async Task<List<MenuOption>> GetAllToolsAsync()
    {
        var tools = new List<MenuOption>();
        
        foreach (var installer in _allInstallers)
        {
            var isInstalled = await installer.IsInstalledAsync();
            tools.Add(new MenuOption(installer.Name, installer, isInstalled));
        }
        
        return tools;
    }

    public static async Task<List<MenuOption>> GetToolsByCategoryAsync(DevelopmentCategory category)
    {
        var tools = new List<MenuOption>();
        
        foreach (var installer in _allInstallers.Where(i => i.Category == category))
        {
            var isInstalled = await installer.IsInstalledAsync();
            tools.Add(new MenuOption(installer.Name, installer, isInstalled));
        }
        
        return tools;
    }

    public static List<MenuOption> GetMainMenuOptions()
    {
        return new List<MenuOption>
        {
            new MenuOption("C# Development", DevelopmentCategory.CSharp, "Tools for C# and .NET development"),
            new MenuOption("Python Development", DevelopmentCategory.Python, "Tools for Python development"),
            new MenuOption("Node.js Development", DevelopmentCategory.NodeJS, "Tools for Node.js and JavaScript development"),
            new MenuOption("Cross-Platform Tools", DevelopmentCategory.CrossPlatform, "Tools that work across multiple platforms")
        };
    }

    public static IInstaller? GetInstallerByName(string name)
    {
        return _allInstallers.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static async Task<Dictionary<DevelopmentCategory, int>> GetCategoryCountsAsync()
    {
        var counts = new Dictionary<DevelopmentCategory, int>();
        
        var categories = new DevelopmentCategory[]
        {
            DevelopmentCategory.CSharp,
            DevelopmentCategory.Python,
            DevelopmentCategory.NodeJS,
            DevelopmentCategory.CrossPlatform
        };
        
        foreach (var category in categories)
        {
            var tools = await GetToolsByCategoryAsync(category);
            counts[category] = tools.Count;
        }
        
        return counts;
    }
}