using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using DevToolInstaller;
using DevToolInstaller.Installers;

namespace DevToolInstaller.Tests.Installers;

public class DotNetSdkInstallerTests : IDisposable
{
    private readonly DotNetSdkInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public DotNetSdkInstallerTests()
    {
        _installer = new DotNetSdkInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal(".NET 8 SDK", _installer.Name);
        Assert.Equal(DevelopmentCategory.CSharp, _installer.Category);
        Assert.NotNull(_installer.Description);
        Assert.NotNull(_installer.Dependencies);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenDotnetNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert - on macOS without dotnet, should return false
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
    public void Dependencies_ReturnsEmptyList()
    {
        // Act
        var dependencies = _installer.Dependencies;

        // Assert
        Assert.NotNull(dependencies);
        Assert.Empty(dependencies);
    }
}

public class DotNetSdk10InstallerTests : IDisposable
{
    private readonly DotNetSdk10Installer _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public DotNetSdk10InstallerTests()
    {
        _installer = new DotNetSdk10Installer();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal(".NET 10 SDK", _installer.Name);
        Assert.Equal(DevelopmentCategory.CSharp, _installer.Category);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenDotnet10NotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstallAsync_ReportsProgress_WhenInstalling()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert
        _progressMock.Verify(x => x.ReportStatus(It.IsAny<string>()), Times.AtLeastOnce);
    }
}

public class NodeJs20InstallerTests : IDisposable
{
    private readonly NodeJs20Installer _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public NodeJs20InstallerTests()
    {
        _installer = new NodeJs20Installer();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Node.js 20", _installer.Name);
        Assert.Equal(DevelopmentCategory.NodeJS, _installer.Category);
        Assert.Contains("NVM for Windows", _installer.Dependencies);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenNodeNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstallAsync_ReportsProgress_WhenInstalling()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert
        _progressMock.Verify(x => x.ReportStatus(It.IsAny<string>()), Times.AtLeastOnce);
    }
}

public class NodeJs22InstallerTests : IDisposable
{
    private readonly NodeJs22Installer _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public NodeJs22InstallerTests()
    {
        _installer = new NodeJs22Installer();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Node.js 22", _installer.Name);
        Assert.Equal(DevelopmentCategory.NodeJS, _installer.Category);
        Assert.Empty(_installer.Dependencies);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenNodeNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstallAsync_ReportsProgress_WhenInstalling()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert
        _progressMock.Verify(x => x.ReportStatus(It.IsAny<string>()), Times.AtLeastOnce);
    }
}

public class NodeJs24InstallerTests : IDisposable
{
    private readonly NodeJs24Installer _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public NodeJs24InstallerTests()
    {
        _installer = new NodeJs24Installer();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Node.js 24", _installer.Name);
        Assert.Equal(DevelopmentCategory.NodeJS, _installer.Category);
        Assert.Contains("Latest Current Release", _installer.Description);
        Assert.Empty(_installer.Dependencies);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenNodeNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstallAsync_ReportsProgress_WhenInstalling()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _installer.InstallAsync(_progressMock.Object, cancellationToken);

        // Assert
        _progressMock.Verify(x => x.ReportStatus(It.IsAny<string>()), Times.AtLeastOnce);
    }
}
