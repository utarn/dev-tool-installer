using System.Net.Http;
using System.Diagnostics;

namespace DevToolInstaller;

public class DownloadProgress
{
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public double ProgressPercentage => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes * 100 : 0;
    public TimeSpan ElapsedTime { get; set; }
    public double DownloadSpeedMBps => ElapsedTime.TotalSeconds > 0 ? BytesDownloaded / 1024.0 / 1024.0 / ElapsedTime.TotalSeconds : 0;
}

public class DownloadManager
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(30)
    };

    public static async Task<string> DownloadFileAsync(string url, string destinationPath, string displayName, CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(url, destinationPath, displayName, null, cancellationToken);
    }

    public static async Task<string> DownloadFileAsync(string url, string destinationPath, string displayName, IProgressReporter? progressReporter, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            progressReporter?.ReportStatus($"Downloading {displayName}...");
            
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var buffer = new byte[8192];
            long totalRead = 0;
            
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            int lastPercentage = -1;
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;
                
                if (totalBytes > 0)
                {
                    var currentPercentage = (int)((double)totalRead / totalBytes * 100);
                    if (currentPercentage != lastPercentage)
                    {
                        lastPercentage = currentPercentage;
                        var speedMBps = totalRead / 1024.0 / 1024.0 / stopwatch.Elapsed.TotalSeconds;
                        var downloadedMB = totalRead / 1024.0 / 1024.0;
                        var totalMB = totalBytes / 1024.0 / 1024.0;
                        
                        if (progressReporter != null)
                        {
                            var status = $"Downloading {displayName}: {currentPercentage}% ({downloadedMB:F2} MB / {totalMB:F2} MB) - Speed: {speedMBps:F2} MB/s";
                            progressReporter.ReportProgress(status, currentPercentage);
                        }
                        else
                        {
                            ConsoleHelper.WriteProgress($"Progress: {currentPercentage}% ({downloadedMB:F2} MB / {totalMB:F2} MB) - Speed: {speedMBps:F2} MB/s");
                        }
                    }
                }
            }
            
            stopwatch.Stop();
            if (progressReporter != null)
            {
                progressReporter.ReportSuccess($"{displayName} downloaded successfully in {stopwatch.Elapsed.TotalSeconds:F2}s");
            }
            else
            {
                ConsoleHelper.WriteSuccess($"{displayName} downloaded successfully in {stopwatch.Elapsed.TotalSeconds:F2}s");
            }
            
            return destinationPath;
        }
        catch (Exception ex)
        {
            if (progressReporter != null)
            {
                progressReporter.ReportError($"Failed to download {displayName}: {ex.Message}");
            }
            else
            {
                ConsoleHelper.WriteError($"Failed to download {displayName}: {ex.Message}");
            }
            throw;
        }
    }

    public static async Task<List<string>> DownloadFilesAsync(Dictionary<string, (string Url, string DisplayName)> downloads, string tempDirectory, CancellationToken cancellationToken = default)
    {
        var results = new List<string>();
        var tasks = new List<Task<string>>();
        
        foreach (var (fileName, (url, displayName)) in downloads)
        {
            var destinationPath = Path.Combine(tempDirectory, fileName);
            tasks.Add(DownloadFileAsync(url, destinationPath, displayName, cancellationToken));
        }
        
        foreach (var task in tasks)
        {
            results.Add(await task);
        }
        
        return results;
    }
}