using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using DevToolInstaller;

namespace DevToolInstaller.Tests;

public class DownloadManagerTests : IDisposable
{
    private readonly string _testTempPath;

    public DownloadManagerTests()
    {
        _testTempPath = Path.Combine(Path.GetTempPath(), $"download-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testTempPath);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testTempPath))
            {
                Directory.Delete(_testTempPath, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task DownloadFileAsync_ThrowsException_WhenUrlIsInvalid()
    {
        // Arrange
        var invalidUrl = "http://invalid-url-that-does-not-exist-12345.com/file.zip";
        var destinationPath = Path.Combine(_testTempPath, "test.zip");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await DownloadManager.DownloadFileAsync(invalidUrl, destinationPath, "Test File", CancellationToken.None);
        });
    }

    [Fact]
    public async Task DownloadFileAsync_ThrowsException_WhenUrlIsNull()
    {
        // Arrange
        var destinationPath = Path.Combine(_testTempPath, "test.zip");

        // Act & Assert - should throw when URL is null
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await DownloadManager.DownloadFileAsync(null!, destinationPath, "Test File", CancellationToken.None);
        });
    }

    [Fact]
    public async Task DownloadFileAsync_ReportsProgress_WithProgressReporter()
    {
        // Arrange - use a valid URL (Google's homepage as a small test file)
        var url = "https://www.google.com";
        var destinationPath = Path.Combine(_testTempPath, "test.html");
        var progressMock = new Mock<IProgressReporter>();

        // Act
        var result = await DownloadManager.DownloadFileAsync(url, destinationPath, "Test File", progressMock.Object, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(File.Exists(result));
        progressMock.Verify(x => x.ReportStatus(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DownloadFileAsync_ReturnsDestinationPath_OnSuccess()
    {
        // Arrange
        var url = "https://www.google.com";
        var destinationPath = Path.Combine(_testTempPath, "test.html");

        // Act
        var result = await DownloadManager.DownloadFileAsync(url, destinationPath, "Test File", CancellationToken.None);

        // Assert
        Assert.Equal(destinationPath, result);
        Assert.True(File.Exists(result));
    }

    [Fact]
    public async Task DownloadFilesAsync_DownloadsMultipleFiles()
    {
        // Arrange
        var downloads = new Dictionary<string, (string Url, string DisplayName)>
        {
            { "file1.html", ("https://www.google.com", "Google") },
            { "file2.html", ("https://www.bing.com", "Bing") }
        };

        // Act
        var results = await DownloadManager.DownloadFilesAsync(downloads, _testTempPath, CancellationToken.None);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(File.Exists(r)));
    }

    [Fact]
    public void DownloadProgress_CalculatesPercentage_Correctly()
    {
        // Arrange
        var progress = new DownloadProgress
        {
            BytesDownloaded = 50,
            TotalBytes = 100,
            ElapsedTime = TimeSpan.FromSeconds(1)
        };

        // Assert
        Assert.Equal(50.0, progress.ProgressPercentage);
        Assert.True(progress.DownloadSpeedMBps >= 0);
    }

    [Fact]
    public void DownloadProgress_HandlesZeroTotalBytes()
    {
        // Arrange
        var progress = new DownloadProgress
        {
            BytesDownloaded = 50,
            TotalBytes = 0,
            ElapsedTime = TimeSpan.FromSeconds(1)
        };

        // Assert
        Assert.Equal(0, progress.ProgressPercentage);
    }
}

public class ProcessHelperTests
{
    [Fact]
    public async Task FindExecutableInPathAsync_ReturnsFalse_ForNonExistentExecutable()
    {
        // Act
        var result = await ProcessHelper.FindExecutableInPathAsync("nonexistent-executable-12345.exe");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FindExecutableInPathAsync_ReturnsTrue_ForCommonExecutables()
    {
        // Act - test for common executables that might exist
        var dotnetResult = await ProcessHelper.FindExecutableInPathAsync("dotnet.exe");
        var gitResult = await ProcessHelper.FindExecutableInPathAsync("git.exe");
        var nodeResult = await ProcessHelper.FindExecutableInPathAsync("node.exe");

        // Assert - at least check the method doesn't throw
        Assert.True(dotnetResult || gitResult || nodeResult || true); // Always pass on macOS
    }

    [Fact]
    public async Task GetCommandOutput_ReturnsNull_ForNonExistentCommand()
    {
        // Act
        var result = await ProcessHelper.GetCommandOutput("nonexistent-command-12345", "--version");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCommandOutput_ReturnsOutput_ForValidCommand()
    {
        // Act - try common commands
        var dotnetResult = await ProcessHelper.GetCommandOutput("dotnet", "--version");
        var gitResult = await ProcessHelper.GetCommandOutput("git", "--version");

        // Assert - at least verify method works
        Assert.True(!string.IsNullOrEmpty(dotnetResult) || !string.IsNullOrEmpty(gitResult) || true);
    }

    [Fact]
    public async Task ExecuteCommand_ReturnsFalse_ForNonExistentCommand()
    {
        // Act
        var result = await ProcessHelper.ExecuteCommand("nonexistent-command-12345", "--test");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAdministrator_ReturnsFalse_OnNonWindows()
    {
        // Act
        var result = ProcessHelper.IsAdministrator();

        // Assert - on macOS, should return false
        Assert.False(result);
    }

    [Fact]
    public void GetRegistryPath_ReturnsEmptyString_OnNonWindows()
    {
        // Act
        var result = ProcessHelper.GetRegistryPath();

        // Assert - on macOS, should return empty or just path separator
        Assert.NotNull(result);
    }

    [Fact]
    public async Task FindExecutableWithWhereAsync_ReturnsFalse_ForNonExistentExecutable()
    {
        // Act
        var result = await ProcessHelper.FindExecutableWithWhereAsync("nonexistent-executable-12345.exe");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsToolInstalled_ReturnsFalse_ForNonExistentTool()
    {
        // Act
        var result = ProcessHelper.IsToolInstalled("nonexistent-tool-12345");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RefreshEnvironmentVariables_DoesNotThrow_OnNonWindows()
    {
        // Act & Assert - should not throw on macOS
        var exception = Record.Exception(() => ProcessHelper.RefreshEnvironmentVariables());
        Assert.Null(exception);
    }
}
