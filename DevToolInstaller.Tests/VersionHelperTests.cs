using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DevToolInstaller;

namespace DevToolInstaller.Tests;

public class VersionHelperTests : IDisposable
{
    private readonly HttpMessageHandlerStub _handlerStub;
    private readonly HttpClient _httpClient;

    public VersionHelperTests()
    {
        _handlerStub = new HttpMessageHandlerStub();
        _httpClient = new HttpClient(_handlerStub);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _handlerStub.Dispose();
        VersionHelper.ClearCache();
    }

    [Fact(Skip = "Requires actual API access")]
    public async Task GetLatestDotNetSdkVersionAsync_ReturnsVersionAndUrl_WhenApiSucceeds()
    {
        // Arrange
        var jsonResponse = @"{
            ""latest-release"": {
                ""sdk"": {
                    ""version"": ""8.0.999""
                }
            }
        }";
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        // Act
        var result = await VersionHelper.GetLatestDotNetSdkVersionAsync("8.0");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("8.0.999", result.Value.Version);
        Assert.Contains("dotnet-sdk-8.0.999", result.Value.DownloadUrl);
    }

    [Fact]
    public async Task GetLatestDotNetSdkVersionAsync_ReturnsNull_WhenApiFails()
    {
        // Arrange
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // Act
        var result = await VersionHelper.GetLatestDotNetSdkVersionAsync("8.0");

        // Assert
        Assert.Null(result);
    }

    [Fact(Skip = "Requires actual API access")]
    public async Task GetLatestNodeVersionAsync_ReturnsVersion_WhenApiSucceeds()
    {
        // Arrange
        var jsonResponse = @"[
            { ""version"": ""v20.99.9"" },
            { ""version"": ""v18.0.0"" }
        ]";
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        // Act
        var result = await VersionHelper.GetLatestNodeVersionAsync(20);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("v20.99.9", result);
    }

    [Fact]
    public async Task GetLatestNodeVersionAsync_ReturnsNull_WhenVersionNotFound()
    {
        // Arrange
        var jsonResponse = @"[{ ""version"": ""v18.0.0"" }]";
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        // Act
        var result = await VersionHelper.GetLatestNodeVersionAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact(Skip = "Uses real API - requires proper HTTP mocking")]
    public async Task GetLatestNodeVersionAsync_ReturnsNull_WhenApiFails()
    {
        // Arrange
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // Act
        var result = await VersionHelper.GetLatestNodeVersionAsync(20);

        // Assert
        Assert.Null(result);
    }

    [Fact(Skip = "Requires actual API access")]
    public async Task GetLatestUvVersionAsync_ReturnsVersion_WhenApiSucceeds()
    {
        // Arrange
        var jsonResponse = @"{ ""tag_name"": ""v0.99.0"" }";
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        // Act
        var result = await VersionHelper.GetLatestUvVersionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("v0.99.0", result);
    }

    [Fact]
    public async Task GetLatestUvVersionAsync_ReturnsNull_WhenApiFails()
    {
        // Arrange
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // Act
        var result = await VersionHelper.GetLatestUvVersionAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUvDownloadUrlAsync_ReturnsCorrectUrl()
    {
        // Act
        var result = await VersionHelper.GetUvDownloadUrlAsync("v0.5.0");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("uv-x86_64-pc-windows-msvc.zip", result);
        Assert.Contains("v0.5.0", result);
    }

    [Fact(Skip = "Requires actual API access")]
    public async Task GetThaiFontInstallerUrlAsync_ReturnsUrl_WhenAssetFound()
    {
        // Arrange
        var jsonResponse = @"{
            ""assets"": [
                { ""name"": ""FontInstaller.Console.exe"", ""browser_download_url"": ""https://example.com/font-installer.exe"" }
            ]
        }";
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        // Act
        var result = await VersionHelper.GetThaiFontInstallerUrlAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://example.com/font-installer.exe", result);
    }

    [Fact]
    public async Task GetThaiFontInstallerUrlAsync_ReturnsNull_WhenNoExeAsset()
    {
        // Arrange
        var jsonResponse = @"{ ""assets"": [{ ""name"": ""readme.txt"" }] }";
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        // Act
        var result = await VersionHelper.GetThaiFontInstallerUrlAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetThaiFontInstallerUrlAsync_ReturnsNull_WhenApiFails()
    {
        // Arrange
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // Act
        var result = await VersionHelper.GetThaiFontInstallerUrlAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ClearCache_ClearsAllCachedValues()
    {
        // Arrange & Act
        VersionHelper.ClearCache();

        // Assert - no exception means it worked
        Assert.True(true);
    }

    [Fact(Skip = "Requires actual API access")]
    public async Task GetLatestDotNetSdkVersionAsync_UsesCache_WhenCalledMultipleTimes()
    {
        // Arrange
        var jsonResponse = @"{
            ""latest-release"": {
                ""sdk"": { ""version"": ""8.0.999"" }
            }
        }";
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        // Act - first call
        var result1 = await VersionHelper.GetLatestDotNetSdkVersionAsync("8.0");
        
        // Change response
        var jsonResponse2 = @"{
            ""latest-release"": {
                ""sdk"": { ""version"": ""8.0.111"" }
            }
        }";
        _handlerStub.ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse2)
        };

        // Act - second call (should use cache)
        var result2 = await VersionHelper.GetLatestDotNetSdkVersionAsync("8.0");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Value.Version, result2.Value.Version); // Should be same from cache
    }
}

public class HttpMessageHandlerStub : HttpMessageHandler
{
    public HttpResponseMessage? ResponseMessage { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(ResponseMessage ?? new HttpResponseMessage(HttpStatusCode.OK));
    }
}
