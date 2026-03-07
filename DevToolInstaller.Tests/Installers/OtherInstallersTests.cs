using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using DevToolInstaller;
using DevToolInstaller.Installers;

namespace DevToolInstaller.Tests.Installers;

public class NvmWindowsInstallerTests : IDisposable
{
    private readonly NvmWindowsInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public NvmWindowsInstallerTests()
    {
        _installer = new NvmWindowsInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("NVM for Windows", _installer.Name);
        Assert.Equal(DevelopmentCategory.NodeJS, _installer.Category);
        Assert.Contains("nvm-windows", _installer.Description);
        Assert.Empty(_installer.Dependencies);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenNvmNotFound()
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

public class NpmInstallerTests : IDisposable
{
    private readonly NpmInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public NpmInstallerTests()
    {
        _installer = new NpmInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("NPM", _installer.Name);
        Assert.Equal(DevelopmentCategory.NodeJS, _installer.Category);
        Assert.Contains("Node.js 20", _installer.Dependencies);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenNpmNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }
}

public class NodeJsToolsInstallerTests : IDisposable
{
    private readonly NodeJsToolsInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public NodeJsToolsInstallerTests()
    {
        _installer = new NodeJsToolsInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Node.js Development Tools", _installer.Name);
        Assert.Equal(DevelopmentCategory.NodeJS, _installer.Category);
        Assert.Contains("Node.js 20", _installer.Dependencies);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenToolsNotFound()
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

public class VSCodeInstallerTests : IDisposable
{
    private readonly VSCodeInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public VSCodeInstallerTests()
    {
        _installer = new VSCodeInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Visual Studio Code", _installer.Name);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenVSCodeNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }
}

public class GitInstallerTests : IDisposable
{
    private readonly GitInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public GitInstallerTests()
    {
        _installer = new GitInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Git", _installer.Name);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenGitNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }
}

public class DockerDesktopInstallerTests : IDisposable
{
    private readonly DockerDesktopInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public DockerDesktopInstallerTests()
    {
        _installer = new DockerDesktopInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Docker Desktop", _installer.Name);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenDockerNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }
}

public class PowerShell7InstallerTests : IDisposable
{
    private readonly PowerShell7Installer _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public PowerShell7InstallerTests()
    {
        _installer = new PowerShell7Installer();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("PowerShell 7", _installer.Name);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenPowerShell7NotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }
}

public class WindowsTerminalInstallerTests : IDisposable
{
    private readonly WindowsTerminalInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public WindowsTerminalInstallerTests()
    {
        _installer = new WindowsTerminalInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Windows Terminal", _installer.Name);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenTerminalNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }
}

public class OhMyPoshInstallerTests : IDisposable
{
    private readonly OhMyPoshInstaller _installer;
    private readonly Mock<IProgressReporter> _progressMock;

    public OhMyPoshInstallerTests()
    {
        _installer = new OhMyPoshInstaller();
        _progressMock = new Mock<IProgressReporter>();
    }

    public void Dispose() { }

    [Fact]
    public void Constructor_InitializesInstaller()
    {
        // Assert
        Assert.NotNull(_installer);
        Assert.Equal("Oh My Posh + Profile", _installer.Name);
        Assert.Equal(DevelopmentCategory.CrossPlatform, _installer.Category);
        Assert.Contains("PowerShell 7", _installer.Dependencies);
    }

    [Fact]
    public async Task IsInstalledAsync_ReturnsFalse_WhenOhMyPoshNotFound()
    {
        // Act
        var result = await _installer.IsInstalledAsync();

        // Assert
        Assert.False(result);
    }
}
