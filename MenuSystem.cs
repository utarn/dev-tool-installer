using System.Linq;
namespace DevToolInstaller;

public class MenuSystem : IDisposable
{
    public MenuState CurrentState { get; private set; } = MenuState.MainMenu;
    public int SelectedIndex { get; private set; } = 0;
    
    private List<CategoryGroup> _categories = new();
    private List<DisplayRow> _displayRows = new();
    private int _scrollOffset = 0;
    private IInstaller? _currentInstaller;
    private bool _forceReinstall = false;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public async Task RunAsync()
    {
        bool isNonInteractive = false;
        try
        {
            var _ = Console.WindowWidth;
            var __ = Console.KeyAvailable;
        }
        catch (IOException) { isNonInteractive = true; }
        catch (InvalidOperationException) { isNonInteractive = true; }

        if (isNonInteractive)
        {
            ConsoleHelper.WriteError("This application requires an interactive console environment.");
            await Task.Delay(3000);
            return;
        }

        // Load all tools and group by category
        await LoadCategoriesAsync();

        ConsoleHelper.ClearScreen();
        
        while (CurrentState != MenuState.Complete)
        {
            try
            {
                await RenderCurrentStateAsync();
                await HandleInputAsync();
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Error: {ex.Message}");
                ConsoleHelper.WriteInfo("Press any key to continue...");
                try { Console.ReadKey(true); }
                catch (InvalidOperationException) { System.Threading.Thread.Sleep(2000); }
            }
        }
    }

    private async Task LoadCategoriesAsync()
    {
        var allTools = await ToolRegistry.GetAllToolsAsync();
        
        var catDefs = new[]
        {
            (DevelopmentCategory.CSharp, "C# Development"),
            (DevelopmentCategory.Python, "Python Development"),
            (DevelopmentCategory.NodeJS, "Node.js Development"),
            (DevelopmentCategory.CrossPlatform, "Cross-Platform Tools"),
        };

        _categories = new List<CategoryGroup>();
        foreach (var (cat, label) in catDefs)
        {
            var tools = allTools.Where(t => t.Installer?.Category == cat).ToList();
            if (tools.Count == 0) continue;
            _categories.Add(new CategoryGroup { Name = label, Category = cat, Tools = tools });
        }
    }

    private async Task RenderCurrentStateAsync()
    {
        ConsoleHelper.ClearScreen();
        
        switch (CurrentState)
        {
            case MenuState.MainMenu:
                RenderCategoryList();
                break;
            case MenuState.Installing:
                await RenderInstallationProgressAsync();
                break;
        }
    }

    private void RenderCategoryList()
    {
        _displayRows = BuildDisplayRows();
        SelectedIndex = Math.Clamp(SelectedIndex, 0, Math.Max(0, _displayRows.Count - 1));

        int windowWidth, windowHeight;
        try { windowWidth = Console.WindowWidth; windowHeight = Console.WindowHeight; }
        catch (IOException) { windowWidth = 80; windowHeight = 24; }

        var width = Math.Min(90, windowWidth - 4);
        var height = windowHeight;
        var startX = Math.Max(0, (windowWidth - width) / 2);
        var startY = 0;

        ConsoleHelper.DrawBorderedBox(startX, startY, width, height, "DevToolInstaller v2.0");

        // Header
        ConsoleHelper.SetCursorPosition(startX + 2, startY + 2);
        ConsoleHelper.WriteHeader("Select tools to install:");

        // Visible area
        var headerRowCount = 4;
        var footerRowCount = 4;
        var visibleCount = height - headerRowCount - footerRowCount;
        if (visibleCount < 1) visibleCount = 1;

        // Scroll: ensure selected row is visible
        if (SelectedIndex < _scrollOffset)
            _scrollOffset = SelectedIndex;
        if (SelectedIndex >= _scrollOffset + visibleCount)
            _scrollOffset = SelectedIndex - visibleCount + 1;
        _scrollOffset = Math.Clamp(_scrollOffset, 0, Math.Max(0, _displayRows.Count - visibleCount));

        // Render visible rows
        int renderY = startY + headerRowCount;
        for (int r = _scrollOffset; r < Math.Min(_scrollOffset + visibleCount, _displayRows.Count); r++)
        {
            ConsoleHelper.SetCursorPosition(startX + 2, renderY);
            var row = _displayRows[r];
            bool isCursor = (r == SelectedIndex);

            if (row.IsCategoryHeader && row.CategoryRef != null)
            {
                RenderCategoryHeaderRow(row.CategoryRef, isCursor);
            }
            else if (row.ToolOption != null)
            {
                RenderToolRow(row.ToolOption, isCursor);
            }

            renderY++;
        }

        // Scroll indicators
        if (_displayRows.Count > visibleCount)
        {
            if (_scrollOffset > 0)
            {
                ConsoleHelper.SetCursorPosition(startX + width - 12, startY + headerRowCount);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("^ scroll up");
                Console.ResetColor();
            }
            if (_scrollOffset + visibleCount < _displayRows.Count)
            {
                ConsoleHelper.SetCursorPosition(startX + width - 14, startY + headerRowCount + visibleCount - 1);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("v scroll down");
                Console.ResetColor();
            }
        }

        // Selected count summary
        var allTools = _categories.SelectMany(c => c.Tools);
        var selectedTools = allTools.Where(t => t.IsSelected).ToList();
        var selectedTotalCount = selectedTools.Count;
        var allToolCount = allTools.Count();
        var pendingToolCount = selectedTools.Count(t => !t.IsInstalled && !(t.Installer?.AlwaysRun == true));

        ConsoleHelper.SetCursorPosition(startX + 4, startY + height - footerRowCount + 1);
        if (selectedTotalCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (_forceReinstall)
            {
                Console.Write($"{selectedTotalCount} tools selected ({selectedTotalCount} to reinstall / {allToolCount} total)");
            }
            else
            {
                Console.Write($"{selectedTotalCount} tools selected ({pendingToolCount} to install / {allToolCount} total)");
            }
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("No tools selected");
            Console.ResetColor();
        }

        // Force reinstall indicator
        if (_forceReinstall)
        {
            ConsoleHelper.SetCursorPosition(startX + 4, startY + height - footerRowCount + 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("! FORCE REINSTALL MODE -- will reinstall already-installed tools");
            Console.ResetColor();
        }

        // Help
        ConsoleHelper.SetCursorPosition(startX + 2, startY + height - 2);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("[↑↓ Navigate] [Space Toggle] [A All] [R Reinstall] [Enter Install] [Esc Exit]");
        Console.ResetColor();
    }

    private void RenderCategoryHeaderRow(CategoryGroup cat, bool isCursor)
    {
        var selectedInCat = cat.Tools.Count(t => t.IsSelected);
        var totalInCat = cat.Tools.Count;

        // Category checkbox: [*] all, [ ] none, [~] partial
        string checkbox;
        if (selectedInCat == totalInCat)
            checkbox = "[*]";
        else if (selectedInCat == 0)
            checkbox = "[ ]";
        else
            checkbox = "[~]";

        var prefix = isCursor ? "> " : "  ";

        if (isCursor)
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (selectedInCat == totalInCat)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        else if (selectedInCat > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
        }

        Console.Write($"{prefix}{checkbox} {cat.Name}");
        Console.ResetColor();

        // Show summary: (X selected / Y total)
        Console.ForegroundColor = ConsoleColor.DarkGray;
        if (_forceReinstall && selectedInCat > 0)
        {
            Console.Write($"  ({selectedInCat} selected / {totalInCat} total - force reinstall)");
        }
        else if (selectedInCat == 0)
        {
            Console.Write($"  ({totalInCat} tools)");
        }
        else
        {
            Console.Write($"  ({selectedInCat} selected / {totalInCat} total)");
        }
        Console.ResetColor();
    }

    private void RenderToolRow(MenuOption tool, bool isCursor)
    {
        var toolCheckbox = tool.IsSelected ? "[x]" : "[ ]";
        var prefix = isCursor ? "    > " : "      ";

        // Prefix + checkbox
        if (isCursor || tool.IsSelected)
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }
        Console.Write($"{prefix}{toolCheckbox} ");

        // Status icon
        if (tool.Installer?.AlwaysRun == true)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("~ ");
        }
        else if (tool.IsInstalled)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("+ ");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("- ");
        }

        // Tool name
        if (isCursor)
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (tool.IsInstalled)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.Write(tool.Text);
        Console.ResetColor();
    }

    private List<DisplayRow> BuildDisplayRows()
    {
        var rows = new List<DisplayRow>();
        foreach (var cat in _categories)
        {
            rows.Add(new DisplayRow { Text = cat.Name, IsCategoryHeader = true, CategoryRef = cat });
            foreach (var tool in cat.Tools)
            {
                rows.Add(new DisplayRow { Text = tool.Text, ToolOption = tool });
            }
        }
        return rows;
    }

    private async Task RenderInstallationProgressAsync()
    {
        if (_currentInstaller == null)
        {
            CurrentState = MenuState.MainMenu;
            return;
        }
        
        int windowWidth, windowHeight;
        try { windowWidth = Console.WindowWidth; windowHeight = Console.WindowHeight; }
        catch (IOException) { windowWidth = 80; windowHeight = 24; }
        
        var width = Math.Min(80, windowWidth - 4);
        var height = 14;
        var startX = Math.Max(0, (windowWidth - width) / 2);
        var startY = Math.Max(0, (windowHeight - height) / 2);

        ConsoleHelper.DrawBorderedBox(startX, startY, width, height, $"Installing: {_currentInstaller.Name}");
        
        ConsoleHelper.SetCursorPosition(startX + 2, startY + 3);
        ConsoleHelper.WriteInfo("Installing...");
        
        ConsoleHelper.SetCursorPosition(startX + 24, startY + 4);
        ConsoleHelper.DrawProgressBar(0, 100, width - 26);
        
        var progressReporter = new MenuProgressReporter(startX, startY, width);
        progressReporter.ReportStatus("Preparing installation...");
        
        try
        {
            var success = await _currentInstaller.InstallAsync(progressReporter, _cancellationTokenSource.Token);
            if (success)
                progressReporter.ReportSuccess("Installation completed successfully!");
            else
                progressReporter.ReportError("Installation failed!");
        }
        catch (OperationCanceledException)
        {
            progressReporter.ReportWarning("Installation cancelled!");
        }
        catch (Exception ex)
        {
            progressReporter.ReportError($"Error - {ex.Message}");
        }

        ConsoleHelper.SetCursorPosition(startX + 2, startY + 12);
        ConsoleHelper.WriteInfo("Press any key to continue...");
        try { Console.ReadKey(true); }
        catch (InvalidOperationException) { await Task.Delay(2000); }
        
        CurrentState = MenuState.MainMenu;
    }

    private async Task HandleInputAsync()
    {
        ConsoleKeyInfo key;
        try { key = Console.ReadKey(true); }
        catch (InvalidOperationException)
        {
            CurrentState = MenuState.Complete;
            return;
        }
        
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if (_displayRows.Count > 0)
                    SelectedIndex = (SelectedIndex - 1 + _displayRows.Count) % _displayRows.Count;
                break;
            case ConsoleKey.DownArrow:
                if (_displayRows.Count > 0)
                    SelectedIndex = (SelectedIndex + 1) % _displayRows.Count;
                break;
            case ConsoleKey.Enter:
                await InstallSelectedAsync();
                break;
            case ConsoleKey.Spacebar:
                if (SelectedIndex >= 0 && SelectedIndex < _displayRows.Count)
                {
                    var row = _displayRows[SelectedIndex];
                    if (row.IsCategoryHeader && row.CategoryRef != null)
                    {
                        // Toggle all tools in this category
                        var anyUnselected = row.CategoryRef.Tools.Any(t => !t.IsSelected);
                        foreach (var tool in row.CategoryRef.Tools)
                            tool.IsSelected = anyUnselected;
                    }
                    else if (row.ToolOption != null)
                    {
                        // Toggle individual tool
                        row.ToolOption.IsSelected = !row.ToolOption.IsSelected;
                    }
                }
                break;
            case ConsoleKey.A:
                var allToolsList = _categories.SelectMany(c => c.Tools).ToList();
                var anyToolUnselected = allToolsList.Any(t => !t.IsSelected);
                foreach (var tool in allToolsList)
                    tool.IsSelected = anyToolUnselected;
                break;
            case ConsoleKey.R:
                _forceReinstall = !_forceReinstall;
                break;
            case ConsoleKey.Escape:
                CurrentState = MenuState.Complete;
                break;
            case ConsoleKey.C when key.Modifiers == ConsoleModifiers.Control:
                _cancellationTokenSource.Cancel();
                CurrentState = MenuState.Complete;
                break;
        }
    }

    private async Task InstallSelectedAsync()
    {
        // Collect individually selected tools that need installation
        var itemsToInstall = _categories
            .SelectMany(c => c.Tools)
            .Where(t => t.IsSelected && t.Installer != null && (_forceReinstall || !t.IsInstalled))
            .ToList();

        if (itemsToInstall.Count == 0)
        {
            // Nothing to install — no tools selected or all selected tools already installed
            return;
        }

        int windowWidth, windowHeight;
        try { windowWidth = Console.WindowWidth; windowHeight = Console.WindowHeight; }
        catch (IOException) { windowWidth = 80; windowHeight = 24; }

        var width = Math.Min(80, windowWidth - 4);
        var startX = Math.Max(0, (windowWidth - width) / 2);

        int successCount = 0;
        int failCount = 0;

        for (int i = 0; i < itemsToInstall.Count; i++)
        {
            var item = itemsToInstall[i];
            var installer = item.Installer!;

            ConsoleHelper.ClearScreen();

            var height = 14;
            var startY = Math.Max(0, (windowHeight - height) / 2);

            ConsoleHelper.DrawBorderedBox(startX, startY, width, height,
                $"[{i + 1}/{itemsToInstall.Count}] Installing: {installer.Name}");

            ConsoleHelper.SetCursorPosition(startX + 2, startY + 3);
            ConsoleHelper.WriteInfo("Installing...");

            ConsoleHelper.SetCursorPosition(startX + 24, startY + 4);
            ConsoleHelper.DrawProgressBar(0, 100, width - 26);

            var progressReporter = new MenuProgressReporter(startX, startY, width);
            progressReporter.ReportStatus("Preparing installation...");

            try
            {
                var success = await installer.InstallAsync(progressReporter, _cancellationTokenSource.Token);
                if (success)
                {
                    successCount++;
                    progressReporter.ReportSuccess($"{installer.Name} completed!");
                }
                else
                {
                    failCount++;
                    progressReporter.ReportError($"{installer.Name} failed!");
                }
            }
            catch (OperationCanceledException)
            {
                progressReporter.ReportWarning("Installation cancelled!");
                break;
            }
            catch (Exception ex)
            {
                failCount++;
                progressReporter.ReportError($"{installer.Name}: {ex.Message}");
            }

            if (i < itemsToInstall.Count - 1)
                await Task.Delay(1500);
        }

        // Summary
        ConsoleHelper.ClearScreen();
        var summaryHeight = 10;
        var summaryY = Math.Max(0, (windowHeight - summaryHeight) / 2);
        ConsoleHelper.DrawBorderedBox(startX, summaryY, width, summaryHeight, "Installation Summary");

        ConsoleHelper.SetCursorPosition(startX + 4, summaryY + 3);
        ConsoleHelper.WriteSuccess($"Succeeded: {successCount}");

        if (failCount > 0)
        {
            ConsoleHelper.SetCursorPosition(startX + 4, summaryY + 4);
            ConsoleHelper.WriteError($"Failed:    {failCount}");
        }

        ConsoleHelper.SetCursorPosition(startX + 4, summaryY + 5);
        ConsoleHelper.WriteInfo($"Total:     {successCount + failCount}/{itemsToInstall.Count}");

        ConsoleHelper.SetCursorPosition(startX + 2, summaryY + summaryHeight - 2);
        ConsoleHelper.WriteInfo("Press any key to continue...");
        try { Console.ReadKey(true); }
        catch (InvalidOperationException) { await Task.Delay(2000); }

        // Prompt to restart computer
        if (successCount > 0)
        {
            await PromptRestartComputerAsync(startX, width, windowHeight);
        }

        // Clear tool selections and refresh tool status
        foreach (var cat in _categories)
            foreach (var tool in cat.Tools)
                tool.IsSelected = false;

        await LoadCategoriesAsync();
    }

    private async Task PromptRestartComputerAsync(int startX, int width, int windowHeight)
    {
        ConsoleHelper.ClearScreen();
        var boxHeight = 10;
        var boxY = Math.Max(0, (windowHeight - boxHeight) / 2);

        ConsoleHelper.DrawBorderedBox(startX, boxY, width, boxHeight, "Restart Required");

        ConsoleHelper.SetCursorPosition(startX + 4, boxY + 3);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("! ");
        Console.ResetColor();
        Console.Write("A restart is recommended to apply all changes.");

        ConsoleHelper.SetCursorPosition(startX + 4, boxY + 4);
        Console.Write("PATH, fonts, and terminal settings require a restart.");

        ConsoleHelper.SetCursorPosition(startX + 4, boxY + 6);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Restart now? [Y/N]: ");
        Console.ResetColor();

        try
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Y)
            {
                ConsoleHelper.SetCursorPosition(startX + 4, boxY + 7);
                ConsoleHelper.WriteInfo("Restarting in 5 seconds... Press any key to cancel.");

                for (int i = 5; i > 0; i--)
                {
                    try
                    {
                        if (Console.KeyAvailable)
                        {
                            Console.ReadKey(true);
                            ConsoleHelper.SetCursorPosition(startX + 4, boxY + 7);
                            ConsoleHelper.WriteWarning("Restart cancelled.");
                            await Task.Delay(1500);
                            return;
                        }
                    }
                    catch (InvalidOperationException) { }

                    ConsoleHelper.SetCursorPosition(startX + 24, boxY + 7);
                    Console.Write($"{i}...");
                    await Task.Delay(1000);
                }

                // Restart the computer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/r /t 0",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                Environment.Exit(0);
            }
            else
            {
                ConsoleHelper.SetCursorPosition(startX + 4, boxY + 7);
                ConsoleHelper.WriteInfo("Skipped restart. Please restart manually for full effect.");
                await Task.Delay(2000);
            }
        }
        catch (InvalidOperationException)
        {
            await Task.Delay(2000);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}

/// <summary>
/// A group of tools under a single development category.
/// </summary>
internal class CategoryGroup
{
    public string Name { get; set; } = "";
    public DevelopmentCategory Category { get; set; }
    public List<MenuOption> Tools { get; set; } = new();
}

/// <summary>
/// A single row in the display list — either a category header or a sub-item tool.
/// </summary>
internal class DisplayRow
{
    public string Text { get; set; } = "";
    public bool IsCategoryHeader { get; set; }
    public CategoryGroup? CategoryRef { get; set; }
    public MenuOption? ToolOption { get; set; }
}