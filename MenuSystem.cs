using System.Linq;
namespace DevToolInstaller;

public class MenuSystem : IDisposable
{
    public MenuState CurrentState { get; private set; } = MenuState.MainMenu;
    public int SelectedIndex { get; private set; } = 0;
    
    private List<MenuOption> _allTools = new();
    private int _scrollOffset = 0;
    private IInstaller? _currentInstaller;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public async Task RunAsync()
    {
        // Check if we're in a non-interactive environment
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

        // Load all tools once at startup
        _allTools = await ToolRegistry.GetAllToolsAsync();

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

    private async Task RenderCurrentStateAsync()
    {
        ConsoleHelper.ClearScreen();
        
        switch (CurrentState)
        {
            case MenuState.MainMenu:
                RenderToolList();
                break;
            case MenuState.Installing:
                await RenderInstallationProgressAsync();
                break;
        }
    }

    private void RenderToolList()
    {
        SelectedIndex = Math.Clamp(SelectedIndex, 0, Math.Max(0, _allTools.Count - 1));

        int windowWidth, windowHeight;
        try { windowWidth = Console.WindowWidth; windowHeight = Console.WindowHeight; }
        catch (IOException) { windowWidth = 80; windowHeight = 24; }

        var width = Math.Min(90, windowWidth - 4);
        var height = Math.Min(windowHeight - 2, windowHeight - 2);
        var startX = Math.Max(0, (windowWidth - width) / 2);
        var startY = 0;

        ConsoleHelper.DrawBorderedBox(startX, startY, width, height, "DevToolInstaller v2.0");

        // Header
        ConsoleHelper.SetCursorPosition(startX + 2, startY + 2);
        ConsoleHelper.WriteHeader("Select tools to install:");

        // Calculate visible area for tool items
        var headerRows = 4; // top border + blank + header + blank
        var footerRows = 4; // selected count + blank + help + bottom border
        var visibleCount = height - headerRows - footerRows;
        if (visibleCount < 1) visibleCount = 1;

        // Adjust scroll offset to keep cursor visible
        if (SelectedIndex < _scrollOffset)
            _scrollOffset = SelectedIndex;
        if (SelectedIndex >= _scrollOffset + visibleCount)
            _scrollOffset = SelectedIndex - visibleCount + 1;

        // Build display rows: group by category with headers
        var displayRows = BuildDisplayRows();

        // Map SelectedIndex (tool index) to display row index
        int cursorDisplayRow = 0;
        int toolIdx = 0;
        for (int r = 0; r < displayRows.Count; r++)
        {
            if (displayRows[r].ToolOption != null)
            {
                if (toolIdx == SelectedIndex)
                {
                    cursorDisplayRow = r;
                    break;
                }
                toolIdx++;
            }
        }

        // Adjust scroll for display rows
        if (cursorDisplayRow < _scrollOffset)
            _scrollOffset = cursorDisplayRow;
        if (cursorDisplayRow >= _scrollOffset + visibleCount)
            _scrollOffset = cursorDisplayRow - visibleCount + 1;
        _scrollOffset = Math.Clamp(_scrollOffset, 0, Math.Max(0, displayRows.Count - visibleCount));

        // Render visible rows
        int renderY = startY + headerRows;
        for (int r = _scrollOffset; r < Math.Min(_scrollOffset + visibleCount, displayRows.Count); r++)
        {
            ConsoleHelper.SetCursorPosition(startX + 2, renderY);
            var row = displayRows[r];

            if (row.IsCategoryHeader)
            {
                // Category header
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                var label = $"── {row.Text} ──";
                Console.Write(label);
                Console.ResetColor();
            }
            else if (row.ToolOption != null)
            {
                bool isCursor = (r == cursorDisplayRow);
                ConsoleHelper.DrawCheckboxItem(
                    row.ToolOption.Text,
                    row.ToolOption.IsSelected,
                    isCursor,
                    row.ToolOption.IsInstalled);
                // DrawCheckboxItem already writes newline, so we move back up
            }

            renderY++;
        }

        // Scroll indicator
        if (displayRows.Count > visibleCount)
        {
            ConsoleHelper.SetCursorPosition(startX + width - 12, startY + headerRows);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (_scrollOffset > 0) Console.Write("▲ scroll up");
            Console.ResetColor();

            ConsoleHelper.SetCursorPosition(startX + width - 14, startY + headerRows + visibleCount - 1);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (_scrollOffset + visibleCount < displayRows.Count) Console.Write("▼ scroll down");
            Console.ResetColor();
        }

        // Selected count
        var selectedCount = _allTools.Count(o => o.IsSelected);
        ConsoleHelper.SetCursorPosition(startX + 4, startY + height - footerRows + 1);
        if (selectedCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{selectedCount} selected");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("No tools selected");
            Console.ResetColor();
        }

        // Help text
        ConsoleHelper.SetCursorPosition(startX + 2, startY + height - 2);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("[↑↓ Navigate] [Space Toggle] [A All] [Enter Install] [Esc Exit]");
        Console.ResetColor();
    }

    private List<DisplayRow> BuildDisplayRows()
    {
        var rows = new List<DisplayRow>();
        var categories = new[]
        {
            (DevelopmentCategory.CSharp, "C# Development"),
            (DevelopmentCategory.Python, "Python Development"),
            (DevelopmentCategory.NodeJS, "Node.js Development"),
            (DevelopmentCategory.CrossPlatform, "Cross-Platform Tools"),
        };

        foreach (var (cat, label) in categories)
        {
            var tools = _allTools.Where(t => t.Installer?.Category == cat).ToList();
            if (tools.Count == 0) continue;

            rows.Add(new DisplayRow { Text = label, IsCategoryHeader = true });
            foreach (var tool in tools)
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

            ConsoleHelper.SetCursorPosition(startX + 2, startY + 12);
            ConsoleHelper.WriteInfo("Press any key to continue...");
            try { Console.ReadKey(true); }
            catch (InvalidOperationException) { await Task.Delay(2000); }
        }
        catch (OperationCanceledException)
        {
            progressReporter.ReportWarning("Installation cancelled!");
            ConsoleHelper.SetCursorPosition(startX + 2, startY + 12);
            ConsoleHelper.WriteInfo("Press any key to continue...");
            try { Console.ReadKey(true); }
            catch (InvalidOperationException) { await Task.Delay(2000); }
        }
        catch (Exception ex)
        {
            progressReporter.ReportError($"Error - {ex.Message}");
            ConsoleHelper.SetCursorPosition(startX + 2, startY + 12);
            ConsoleHelper.WriteInfo("Press any key to continue...");
            try { Console.ReadKey(true); }
            catch (InvalidOperationException) { await Task.Delay(2000); }
        }
        
        CurrentState = MenuState.MainMenu;
    }

    private async Task HandleInputAsync()
    {
        ConsoleKeyInfo key;
        try { key = Console.ReadKey(true); }
        catch (InvalidOperationException)
        {
            CurrentState = MenuState.Complete;
            ConsoleHelper.WriteError("Console input not available. Exiting...");
            await Task.Delay(2000);
            return;
        }
        
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                NavigateUp();
                break;
            case ConsoleKey.DownArrow:
                NavigateDown();
                break;
            case ConsoleKey.Enter:
                await SelectCurrentItem();
                break;
            case ConsoleKey.Spacebar:
                ToggleCurrentItem();
                break;
            case ConsoleKey.A:
                ToggleAllItems();
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

    private void NavigateUp()
    {
        if (_allTools.Count > 0)
            SelectedIndex = (SelectedIndex - 1 + _allTools.Count) % _allTools.Count;
    }

    private void NavigateDown()
    {
        if (_allTools.Count > 0)
            SelectedIndex = (SelectedIndex + 1) % _allTools.Count;
    }

    private async Task SelectCurrentItem()
    {
        if (CurrentState != MenuState.MainMenu) return;

        var selectedItems = _allTools.Where(o => o.IsSelected && o.Installer != null).ToList();
        if (selectedItems.Count > 0)
        {
            await BatchInstallSelectedAsync();
        }
        else if (SelectedIndex >= 0 && SelectedIndex < _allTools.Count && _allTools[SelectedIndex].Installer != null)
        {
            // Fallback: install current highlighted item
            await InitiateInstallation(_allTools[SelectedIndex].Installer!);
        }
    }
    
    private async Task InitiateInstallation(IInstaller installer)
    {
        if (installer.Dependencies.Any())
        {
            foreach (var depName in installer.Dependencies)
            {
                var depInstaller = ToolRegistry.GetInstallerByName(depName);
                if (depInstaller != null)
                {
                    var isInstalled = await depInstaller.IsInstalledAsync();
                    if (!isInstalled)
                    {
                        ConsoleHelper.ClearScreen();
                        ConsoleHelper.WriteError($"Dependency not met: '{depName}' is not installed.");
                        ConsoleHelper.WriteInfo($"Please install '{depName}' before proceeding.");
                        ConsoleHelper.WriteInfo("Press any key to continue...");
                        try { Console.ReadKey(true); }
                        catch (InvalidOperationException) { await Task.Delay(2000); }
                        return;
                    }
                }
            }
        }

        CurrentState = MenuState.Installing;
        _currentInstaller = installer;
    }

    private void ToggleCurrentItem()
    {
        if (CurrentState == MenuState.MainMenu &&
            SelectedIndex >= 0 && SelectedIndex < _allTools.Count)
        {
            _allTools[SelectedIndex].IsSelected = !_allTools[SelectedIndex].IsSelected;
        }
    }

    private void ToggleAllItems()
    {
        if (CurrentState == MenuState.MainMenu)
        {
            var anyUnselected = _allTools.Any(o => !o.IsSelected);
            foreach (var option in _allTools)
                option.IsSelected = anyUnselected;
        }
    }

    private async Task BatchInstallSelectedAsync()
    {
        var itemsToInstall = _allTools
            .Where(o => o.IsSelected && o.Installer != null)
            .ToList();

        if (itemsToInstall.Count == 0) return;

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

        // Show summary
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

        // Clear selections and refresh installed status
        foreach (var option in _allTools)
            option.IsSelected = false;

        _allTools = await ToolRegistry.GetAllToolsAsync();
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}

/// <summary>
/// Represents a single row in the flat tool list display.
/// Can be a category header (non-selectable) or a tool item.
/// </summary>
internal class DisplayRow
{
    public string Text { get; set; } = "";
    public bool IsCategoryHeader { get; set; }
    public MenuOption? ToolOption { get; set; }
}