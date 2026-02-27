using System.Linq;
namespace DevToolInstaller;

public class MenuSystem : IDisposable
{
    public MenuState CurrentState { get; private set; } = MenuState.MainMenu;
    public int SelectedIndex { get; private set; } = 0;
    public Stack<MenuState> StateHistory { get; private set; } = new();
    
    private List<MenuOption> _currentOptions = new();
    private DevelopmentCategory? _currentCategory;
    private IInstaller? _currentInstaller;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public async Task RunAsync()
    {
        // Check if we're in a non-interactive environment
        bool isNonInteractive = false;
        try
        {
            var _ = Console.WindowWidth; // Test console access
            var __ = Console.KeyAvailable; // Test console input
        }
        catch (IOException)
        {
            isNonInteractive = true;
        }
        catch (InvalidOperationException)
        {
            isNonInteractive = true;
        }

        if (isNonInteractive)
        {
            ConsoleHelper.WriteError("This application requires an interactive console environment.");
            ConsoleHelper.WriteError("Console operations are not available in the current environment.");
            await Task.Delay(3000);
            return;
        }

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
                try
                {
                    Console.ReadKey(true);
                }
                catch (InvalidOperationException)
                {
                    // Handle cases where console input is not available (e.g., running in non-interactive environment)
                    // Wait a moment instead of reading a key
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }
    }

    private async Task RenderCurrentStateAsync()
    {
        ConsoleHelper.ClearScreen();
        
        switch (CurrentState)
        {
            case MenuState.MainMenu:
                RenderMainMenu();
                break;
            case MenuState.CategoryMenu:
                RenderCategoryMenu();
                break;
            case MenuState.Installing:
                await RenderInstallationProgressAsync();
                break;
        }
    }

    private void RenderMainMenu()
    {
        _currentOptions = ToolRegistry.GetMainMenuOptions();
        SelectedIndex = Math.Min(SelectedIndex, _currentOptions.Count - 1);
        
        int windowWidth, windowHeight;
        try
        {
            windowWidth = Console.WindowWidth;
            windowHeight = Console.WindowHeight;
        }
        catch (IOException)
        {
            // Fallback dimensions when console properties are not available
            windowWidth = 80;
            windowHeight = 24;
        }
        
        var width = Math.Min(80, windowWidth - 4);
        var height = Math.Min(20, windowHeight - 4);
        var startX = Math.Max(0, (windowWidth - width) / 2);
        var startY = Math.Max(0, (windowHeight - height) / 2);
        
        ConsoleHelper.DrawBorderedBox(startX, startY, width, height, "DevToolInstaller v2.0");
        
        ConsoleHelper.SetCursorPosition(startX + 2, startY + 2);
        ConsoleHelper.WriteHeader("Development Environment Setup");
        
        ConsoleHelper.SetCursorPosition(startX + 2, startY + 4);
        ConsoleHelper.WriteInfo("Select Development Environment:");
        
        for (int i = 0; i < _currentOptions.Count; i++)
        {
            ConsoleHelper.SetCursorPosition(startX + 4, startY + 6 + i);
            ConsoleHelper.DrawMenuItem(_currentOptions[i].Text, i == SelectedIndex);
        }
        
        var helpText = new[]
        {
            "[↑↓ Navigate] [Enter Select] [Esc Exit]"
        };
        
        ConsoleHelper.SetCursorPosition(startX + 2, startY + height - 2);
        ConsoleHelper.ShowHelpText(helpText);
    }

    private void RenderCategoryMenu()
    {
        if (!_currentCategory.HasValue)
        {
            CurrentState = MenuState.MainMenu;
            return;
        }
        
        SelectedIndex = Math.Min(SelectedIndex, _currentOptions.Count - 1);
        
        int windowWidth, windowHeight;
        try
        {
            windowWidth = Console.WindowWidth;
            windowHeight = Console.WindowHeight;
        }
        catch (IOException)
        {
            // Fallback dimensions when console properties are not available
            windowWidth = 80;
            windowHeight = 24;
        }
        
        var width = Math.Min(80, windowWidth - 4);
        var height = Math.Min(20, windowHeight - 4);
        var startX = Math.Max(0, (windowWidth - width) / 2);
        var startY = Math.Max(0, (windowHeight - height) / 2);
        
        var categoryTitle = _currentCategory.Value switch
        {
            DevelopmentCategory.CSharp => "C# Development",
            DevelopmentCategory.Python => "Python Development",
            DevelopmentCategory.NodeJS => "Node.js Development",
            DevelopmentCategory.CrossPlatform => "Cross-Platform Tools",
            _ => "Unknown Category"
        };
        
        ConsoleHelper.DrawBorderedBox(startX, startY, width, height, categoryTitle);
        
        ConsoleHelper.SetCursorPosition(startX + 2, startY + 2);
        ConsoleHelper.WriteInfo("Select tool to install:");
        
        for (int i = 0; i < _currentOptions.Count; i++)
        {
            ConsoleHelper.SetCursorPosition(startX + 4, startY + 4 + i);
            ConsoleHelper.DrawCheckboxItem(
                _currentOptions[i].Text,
                _currentOptions[i].IsSelected,
                i == SelectedIndex,
                _currentOptions[i].IsInstalled);
        }

        // Show selected count
        var selectedCount = _currentOptions.Count(o => o.IsSelected);
        if (selectedCount > 0)
        {
            ConsoleHelper.SetCursorPosition(startX + 4, startY + 5 + _currentOptions.Count);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{selectedCount} selected");
            Console.ResetColor();
        }

        var helpText = new[]
        {
            "[↑↓ Navigate] [Space Toggle] [A Select All] [Enter Install] [Esc Back]"
        };
        
        ConsoleHelper.SetCursorPosition(startX + 2, startY + height - 2);
        ConsoleHelper.ShowHelpText(helpText);
    }

    private async Task RenderInstallationProgressAsync()
    {
        if (_currentInstaller == null)
        {
            CurrentState = MenuState.MainMenu;
            return;
        }
        
        int windowWidth, windowHeight;
        try
        {
            windowWidth = Console.WindowWidth;
            windowHeight = Console.WindowHeight;
        }
        catch (IOException)
        {
            // Fallback dimensions when console properties are not available
            windowWidth = 80;
            windowHeight = 24;
        }
        
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
            {
                progressReporter.ReportSuccess("Installation completed successfully!");
                
                ConsoleHelper.SetCursorPosition(startX + 2, startY + 12);
                ConsoleHelper.WriteInfo("Press any key to continue...");
                try
                {
                    Console.ReadKey(true);
                }
                catch (InvalidOperationException)
                {
                    // Handle cases where console input is not available
                    await Task.Delay(2000);
                }
            }
            else
            {
                progressReporter.ReportError("Installation failed!");
                
                ConsoleHelper.SetCursorPosition(startX + 2, startY + 12);
                ConsoleHelper.WriteInfo("Press any key to continue...");
                try
                {
                    Console.ReadKey(true);
                }
                catch (InvalidOperationException)
                {
                    // Handle cases where console input is not available
                    await Task.Delay(2000);
                }
            }
        }
        catch (OperationCanceledException)
        {
            progressReporter.ReportWarning("Installation cancelled!");
            
            ConsoleHelper.SetCursorPosition(startX + 2, startY + 12);
            ConsoleHelper.WriteInfo("Press any key to continue...");
            try
            {
                Console.ReadKey(true);
            }
            catch (InvalidOperationException)
            {
                // Handle cases where console input is not available
                await Task.Delay(2000);
            }
        }
        catch (Exception ex)
        {
            progressReporter.ReportError($"Error - {ex.Message}");
            
            ConsoleHelper.SetCursorPosition(startX + 2, startY + 12);
            ConsoleHelper.WriteInfo("Press any key to continue...");
            try
            {
                Console.ReadKey(true);
            }
            catch (InvalidOperationException)
            {
                // Handle cases where console input is not available
                await Task.Delay(2000);
            }
        }
        
        // Go back to category menu after installation
        if (StateHistory.Count > 0)
        {
            CurrentState = StateHistory.Pop();
        }
        else
        {
            CurrentState = MenuState.MainMenu;
        }
    }

    private async Task HandleInputAsync()
    {
        ConsoleKeyInfo key;
        try
        {
            key = Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            // Handle cases where console input is not available (e.g., running in non-interactive environment)
            // Exit gracefully since we can't handle interactive input
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
                GoBack();
                break;
            case ConsoleKey.C when key.Modifiers == ConsoleModifiers.Control:
                _cancellationTokenSource.Cancel();
                CurrentState = MenuState.Complete;
                break;
        }
    }

    private void NavigateUp()
    {
        if (_currentOptions.Count > 0)
        {
            SelectedIndex = (SelectedIndex - 1 + _currentOptions.Count) % _currentOptions.Count;
        }
    }

    private void NavigateDown()
    {
        if (_currentOptions.Count > 0)
        {
            SelectedIndex = (SelectedIndex + 1) % _currentOptions.Count;
        }
    }

    private async Task SelectCurrentItem()
    {
        if (SelectedIndex < 0 || SelectedIndex >= _currentOptions.Count)
            return;
        
        var selectedOption = _currentOptions[SelectedIndex];
        
        switch (CurrentState)
        {
            case MenuState.MainMenu:
                if (selectedOption.Category.HasValue)
                {
                    StateHistory.Push(CurrentState);
                    CurrentState = MenuState.CategoryMenu;
                    _currentCategory = selectedOption.Category.Value;
                    _currentOptions = await ToolRegistry.GetToolsByCategoryAsync(_currentCategory.Value);
                    SelectedIndex = 0;
                }
                break;
            case MenuState.CategoryMenu:
                var selectedItems = _currentOptions.Where(o => o.IsSelected && o.Installer != null).ToList();
                if (selectedItems.Count > 0)
                {
                    // Batch install all checked items
                    await BatchInstallSelectedAsync();
                }
                else if (selectedOption.Installer != null)
                {
                    // Fallback: install current highlighted item if nothing checked
                    await InitiateInstallation(selectedOption.Installer);
                }
                break;
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
                    // Here we're not passing the silent flag, so it might print warnings.
                    // This is a known issue with the current implementation of IsInstalledAsync.
                    var isInstalled = await depInstaller.IsInstalledAsync();
                    if (!isInstalled)
                    {
                        ConsoleHelper.ClearScreen();
                        ConsoleHelper.WriteError($"Dependency not met: '{depName}' is not installed.");
                        ConsoleHelper.WriteInfo($"Please install '{depName}' before proceeding.");
                        ConsoleHelper.WriteInfo("Press any key to continue...");
                        try
                        {
                            Console.ReadKey(true);
                        }
                        catch (InvalidOperationException)
                        {
                            // Handle cases where console input is not available
                            await Task.Delay(2000);
                        }
                        return;
                    }
                }
            }
        }

        StateHistory.Push(CurrentState);
        CurrentState = MenuState.Installing;
        _currentInstaller = installer;
        SelectedIndex = 0;
    }

    private void ToggleCurrentItem()
    {
        if (CurrentState == MenuState.CategoryMenu &&
            SelectedIndex >= 0 && SelectedIndex < _currentOptions.Count)
        {
            _currentOptions[SelectedIndex].IsSelected = !_currentOptions[SelectedIndex].IsSelected;
        }
    }

    private void ToggleAllItems()
    {
        if (CurrentState == MenuState.CategoryMenu)
        {
            var anyUnselected = _currentOptions.Any(o => !o.IsSelected);
            foreach (var option in _currentOptions)
            {
                option.IsSelected = anyUnselected;
            }
        }
    }

    private async Task BatchInstallSelectedAsync()
    {
        var itemsToInstall = _currentOptions
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

            // Brief pause between items so user can see result
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
        foreach (var option in _currentOptions)
            option.IsSelected = false;

        if (_currentCategory.HasValue)
            _currentOptions = await ToolRegistry.GetToolsByCategoryAsync(_currentCategory.Value);
    }

    private void GoBack()
    {
        if (StateHistory.Count > 0)
        {
            CurrentState = StateHistory.Pop();
            SelectedIndex = 0;
            
            if (CurrentState == MenuState.MainMenu)
            {
                _currentCategory = null;
            }
        }
        else
        {
            CurrentState = MenuState.Complete;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}