using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using DevToolInstaller;
using DevToolInstaller.Installers;

namespace DevToolInstaller.Tests.Installers;

public class FontInstallerTests : IDisposable
{
    private readonly FontInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public FontInstallerTests()
    {
        _installer = new FontInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("CascadiaMono Nerd Font", _installer.Name);
        Assert.Equal(DevelopmentCategory.CrossPlatform, _installer.Category);
        Assert.Contains("CascadiaMono", _installer.Description);
        Assert.Empty(_installer.Dependencies);
        Assert.True(_installer.AlwaysRun);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_Always()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert - FontInstaller always returns false to allow re-installation
        Assert.False(result);
    }

    [Fact]
    public async Task InstallAsync_ReportsProgress_WhenInstalling()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert - will fail on macOS but should have reported progress
        _progressMock.Verify(x => x.ReportStatus(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task InstallAsync_ReturnsFalse_OnNonWindows()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert - returns false on non-Windows platforms
        Assert.False(result);
    }
}

public class ThaiFontInstallerTests : IDisposable
{
    private readonly ThaiFontInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public ThaiFontInstallerTests()
    {
        _installer = new ThaiFontInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Thai Fonts", _installer.Name);
        Assert.Equal(DevelopmentCategory.CrossPlatform, _installer.Category);
        Assert.Contains("Thai", _installer.Description);
        Assert.Empty(_installer.Dependencies);
        Assert.False(_installer.AlwaysRun);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_OnNonWindows()
    {
        // Act - on non-Windows, IsInstalledAsync returns false (no exception)
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstallAsync_ReportsStatus_WhenInstalling()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert - should report initial status regardless of platform
        _progressMock.Verify(x => x.ReportStatus(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task InstallAsync_ReturnsFalse_OnNonWindows()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert - returns false on non-Windows platforms
        Assert.False(result);
    }

    [Fact]
    public async Task InstallAsync_ReportsWarning_OnNonWindows()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert - should report a warning about Windows-only support
        _progressMock.Verify(x => x.ReportWarning(It.Is<string>(s => s.Contains("Windows"))), Times.Once);
    }

    [Fact]
    public void Description_ContainsFontFamilyNames()
    {
        // Assert - description should mention key Thai font families
        Assert.Contains("Sarabun", _installer.Description);
        Assert.Contains("Chakra Petch", _installer.Description);
    }
}