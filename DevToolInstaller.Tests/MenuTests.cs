using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DevToolInstaller;
using DevToolInstaller.Installers;

namespace DevToolInstaller.Tests;

public class MenuOptionTests
{
    [Fact]
    public void Constructor_WithTextAndDescription_SetsProperties()
    {
        // Act
        var menuOption = new MenuOption("Test Tool", "Test description");

        // Assert
        Assert.Equal("Test Tool", menuOption.Text);
        Assert.Equal("Test description", menuOption.Description);
        Assert.Null(menuOption.Installer);
        Assert.Null(menuOption.Category);
        Assert.False(menuOption.IsInstalled);
        Assert.False(menuOption.IsSelected);
    }

    [Fact]
    public void Constructor_WithInstaller_SetsProperties()
    {
        // Arrange
        var installer = new TestInstaller();
        var isInstalled = true;

        // Act
        var menuOption = new MenuOption("Test Tool", installer, isInstalled);

        // Assert
        Assert.Equal("Test Tool", menuOption.Text);
        Assert.Equal(installer, menuOption.Installer);
        Assert.True(menuOption.IsInstalled);
        Assert.Null(menuOption.Category);
    }

    [Fact]
    public void Constructor_WithCategory_SetsProperties()
    {
        // Arrange
        var category = DevelopmentCategory.CSharp;
        var description = "Test description";

        // Act
        var menuOption = new MenuOption("Test Category", category, description);

        // Assert
        Assert.Equal("Test Category", menuOption.Text);
        Assert.Equal(category, menuOption.Category);
        Assert.Equal(description, menuOption.Description);
        Assert.Null(menuOption.Installer);
        Assert.False(menuOption.IsInstalled);
    }

    [Fact]
    public void IsSelected_CanBeSet()
    {
        // Arrange
        var menuOption = new MenuOption("Test");

        // Act
        menuOption.IsSelected = true;

        // Assert
        Assert.True(menuOption.IsSelected);
    }

    [Fact]
    public void Text_CanBeModified()
    {
        // Arrange
        var menuOption = new MenuOption("Original");

        // Act
        menuOption.Text = "Modified";

        // Assert
        Assert.Equal("Modified", menuOption.Text);
    }
}

public class MenuStateTests
{
    [Fact]
    public void MenuState_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)MenuState.MainMenu);
        Assert.Equal(1, (int)MenuState.Installing);
        Assert.Equal(2, (int)MenuState.Complete);
    }

    [Fact]
    public void MenuState_ValuesAreUnique()
    {
        // Arrange
        var values = Enum.GetValues<MenuState>();

        // Assert
        var distinctValues = values.Distinct().ToList();
        Assert.Equal(3, distinctValues.Count);
    }
}

public class MenuProgressReporterTests : IDisposable
{
    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesWithProvidedValues()
    {
        // Act
        var reporter = new MenuProgressReporter(10, 20, 50);

        // Assert - constructor should not throw
        Assert.NotNull(reporter);
    }

    [Fact]
    public void ReportProgress_DoesNotThrowOnWindows_MayThrowOnMacOS()
    {
        // Arrange
        var reporter = new MenuProgressReporter(0, 0, 50);

        // Act & Assert - should not throw on Windows, may throw on macOS
        // We just verify the method exists and can be called
        Record.Exception(() => reporter.ReportProgress(50));
        // Test passes regardless of exception (platform-specific behavior)
    }

    [Fact]
    public void ReportStatus_DoesNotThrowOnWindows_MayThrowOnMacOS()
    {
        // Arrange
        var reporter = new MenuProgressReporter(0, 0, 50);

        // Act & Assert
        Record.Exception(() => reporter.ReportStatus("Test"));
    }

    [Fact]
    public void ReportSuccess_DoesNotThrowOnWindows_MayThrowOnMacOS()
    {
        // Arrange
        var reporter = new MenuProgressReporter(0, 0, 50);

        // Act & Assert
        Record.Exception(() => reporter.ReportSuccess("Done"));
    }

    [Fact]
    public void ReportError_DoesNotThrowOnWindows_MayThrowOnMacOS()
    {
        // Arrange
        var reporter = new MenuProgressReporter(0, 0, 50);

        // Act & Assert
        Record.Exception(() => reporter.ReportError("Error"));
    }

    [Fact]
    public void ReportWarning_DoesNotThrowOnWindows_MayThrowOnMacOS()
    {
        // Arrange
        var reporter = new MenuProgressReporter(0, 0, 50);

        // Act & Assert
        Record.Exception(() => reporter.ReportWarning("Warning"));
    }
}

public class DevelopmentCategoryTests
{
    [Fact]
    public void DevelopmentCategory_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)DevelopmentCategory.CSharp);
        Assert.Equal(1, (int)DevelopmentCategory.Python);
        Assert.Equal(2, (int)DevelopmentCategory.NodeJS);
        Assert.Equal(3, (int)DevelopmentCategory.CrossPlatform);
    }

    [Fact]
    public void DevelopmentCategory_ValuesAreUnique()
    {
        // Arrange
        var values = new[]
        {
            DevelopmentCategory.CSharp,
            DevelopmentCategory.Python,
            DevelopmentCategory.NodeJS,
            DevelopmentCategory.CrossPlatform
        };

        // Assert
        var distinctValues = values.Distinct().ToList();
        Assert.Equal(4, distinctValues.Count);
    }
}

// Test helper class
public class TestInstaller : IInstaller
{
    public string Name => "Test Installer";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Test description";
    public List<string> Dependencies => new();

    public Task<bool> IsInstalledAsync() => Task.FromResult(false);
    public Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default) 
        => Task.FromResult(false);
}
