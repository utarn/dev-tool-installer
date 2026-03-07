using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevToolInstaller;

/// <summary>
/// Helper class for fetching latest software versions from official release APIs.
/// </summary>
public static class VersionHelper
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Cache for version info to avoid repeated API calls
    private static readonly Dictionary<string, (string Version, DateTime CachedAt)> _versionCache = new();
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets the latest .NET SDK version for a specific major version.
    /// </summary>
    /// <param name="majorVersion">Major version number (e.g., "8.0", "10.0")</param>
    /// <returns>Tuple of (version, downloadUrl) or null if fetch fails</returns>
    public static async Task<(string Version, string DownloadUrl)?> GetLatestDotNetSdkVersionAsync(string majorVersion)
    {
        var cacheKey = $"dotnet-{majorVersion}";
        if (TryGetFromCache(cacheKey, out var cached))
        {
            return (cached.Version, cached.Version); // Return cached version (cache stores version only for non-dotnet)
        }

        try
        {
            var releasesUrl = $"https://dotnetcli.azureedge.net/dotnet/release-metadata/{majorVersion}/releases.json";
            var json = await _httpClient.GetStringAsync(releasesUrl);
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;
            
            // Get the latest release
            if (root.TryGetProperty("latest-release", out var latestReleaseElement) &&
                latestReleaseElement.TryGetProperty("sdk", out var sdkElement) &&
                sdkElement.TryGetProperty("version", out var versionElement))
            {
                var version = versionElement.GetString()!;
                
                // Determine architecture for download URL
                var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
                var downloadUrl = $"https://builds.dotnet.microsoft.com/dotnet/Sdk/{version}/dotnet-sdk-{version}-win-{arch}.exe";
                
                var result = (version, downloadUrl);
                _versionCache[cacheKey] = (version, DateTime.UtcNow);
                return result;
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to fetch .NET {majorVersion} version: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Gets the latest Node.js version for a specific major version.
    /// </summary>
    /// <param name="majorVersion">Major version number (e.g., 20, 22, 24)</param>
    /// <returns>Version string or null if fetch fails</returns>
    public static async Task<string?> GetLatestNodeVersionAsync(int majorVersion)
    {
        var cacheKey = $"node-{majorVersion}";
        if (TryGetFromCache(cacheKey, out var cached))
        {
            return cached.Version;
        }

        try
        {
            var json = await _httpClient.GetStringAsync("https://nodejs.org/dist/index.json");
            using var doc = JsonDocument.Parse(json);

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("version", out var versionElement))
                {
                    var version = versionElement.GetString()!;
                    // Version format is "v20.19.6"
                    if (version.StartsWith($"v{majorVersion}."))
                    {
                        _versionCache[cacheKey] = (version, DateTime.UtcNow);
                        return version;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to fetch Node.js {majorVersion} version: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Gets the latest uv version from GitHub releases.
    /// </summary>
    /// <returns>Version string or null if fetch fails</returns>
    public static async Task<string?> GetLatestUvVersionAsync()
    {
        var cacheKey = "uv-latest";
        if (TryGetFromCache(cacheKey, out var cached))
        {
            return cached.Version;
        }

        try
        {
            // GitHub API for latest release
            var json = await _httpClient.GetStringAsync("https://api.github.com/repos/astral-sh/uv/releases/latest");
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("tag_name", out var tagElement))
            {
                var version = tagElement.GetString()!;
                _versionCache[cacheKey] = (version, DateTime.UtcNow);
                return version;
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to fetch uv version: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Gets the download URL for a specific uv version on Windows x64.
    /// </summary>
    /// <param name="version">Version string (e.g., "0.5.0")</param>
    /// <returns>Download URL or null if not found</returns>
    public static async Task<string?> GetUvDownloadUrlAsync(string version)
    {
        // uv provides direct download URLs in a predictable format
        // https://github.com/astral-sh/uv/releases/download/{version}/uv-x86_64-pc-windows-msvc.zip
        var cleanVersion = version.TrimStart('v');
        return $"https://github.com/astral-sh/uv/releases/download/{version}/uv-x86_64-pc-windows-msvc.zip";
    }

    /// <summary>
    /// Gets the latest Thai Font Installer release from GitHub.
    /// </summary>
    /// <returns>Download URL for the Windows executable or null if fetch fails</returns>
    public static async Task<string?> GetThaiFontInstallerUrlAsync()
    {
        var cacheKey = "thai-font-installer";
        if (TryGetFromCache(cacheKey, out var cached))
        {
            return cached.Version;
        }

        try
        {
            // Assuming font-installer is in the user's local projects, we'll use a local path
            // For production, this would be a GitHub releases URL
            var json = await _httpClient.GetStringAsync("https://api.github.com/repos/utarn/font-installer/releases/latest");
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("assets", out var assetsElement))
            {
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    if (asset.TryGetProperty("name", out var nameElement) &&
                        nameElement.GetString()?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true &&
                        asset.TryGetProperty("browser_download_url", out var urlElement))
                    {
                        var url = urlElement.GetString()!;
                        _versionCache[cacheKey] = (url, DateTime.UtcNow);
                        return url;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to fetch Thai Font Installer URL: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Tries to get a cached version if it's still valid.
    /// </summary>
    private static bool TryGetFromCache(string key, out (string Version, DateTime CachedAt) value)
    {
        if (_versionCache.TryGetValue(key, out var cached) &&
            DateTime.UtcNow - cached.CachedAt < _cacheDuration)
        {
            value = cached;
            return true;
        }
        
        value = default;
        return false;
    }

    /// <summary>
    /// Clears all cached version information.
    /// </summary>
    public static void ClearCache()
    {
        _versionCache.Clear();
    }
}
