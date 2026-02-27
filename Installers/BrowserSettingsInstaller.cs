using Microsoft.Win32;

namespace DevToolInstaller.Installers;

/// <summary>
/// Configures Chromium-based browser settings via Windows Registry Policies (HKCU).
/// Applies to Google Chrome, Microsoft Edge, and Brave Browser.
/// Settings: ask download location, disable background mode, disable analytics,
/// disable startup boost, disable auto-update notifications.
/// </summary>
public class BrowserSettingsInstaller : IInstaller
{
    public string Name => "Browser Privacy Settings";
    public DevelopmentCategory Category => DevelopmentCategory.CrossPlatform;
    public string Description => "Configure browsers: ask download, disable background/analytics/startup boost, remove startup entries";
    public List<string> Dependencies => new();

    private static readonly (string Name, string PolicyPath)[] ChromiumBrowsers =
    [
        ("Google Chrome", @"SOFTWARE\Policies\Google\Chrome"),
        ("Microsoft Edge", @"SOFTWARE\Policies\Microsoft\Edge"),
        ("Brave", @"SOFTWARE\Policies\BraveSoftware\Brave"),
        ("Opera", @"SOFTWARE\Policies\Opera Software\Opera"),
    ];

    /// <summary>
    /// Known startup registry value name prefixes for Chromium browsers.
    /// These are created when browsers enable "Continue running background apps" or "Startup boost".
    /// Located in HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
    /// </summary>
    private static readonly string[] StartupValuePrefixes =
    [
        "GoogleChromeAutoLaunch",
        "MicrosoftEdgeAutoLaunch",
        "BraveSoftware",
        "Opera Browser Assistant",
    ];

    /// <summary>
    /// Common Chromium policy DWORD values applied to all browsers.
    /// Reference: https://chromeenterprise.google/policies/
    /// </summary>
    private static readonly (string Key, string Description, int Value)[] PolicySettings =
    [
        ("PromptForDownloadLocation", "Ask where to save each download", 1),
        ("BackgroundModeEnabled", "Disable background mode (close completely)", 0),
        ("MetricsReportingEnabled", "Disable usage analytics & crash reporting", 0),
        ("StartupBoostEnabled", "Disable startup boost (pre-launch on login)", 0),
        ("AutofillAddressEnabled", "Disable address autofill", 0),
        ("AutofillCreditCardEnabled", "Disable credit card autofill", 0),
        ("PasswordManagerEnabled", "Disable built-in password manager", 0),
        ("HardwareAccelerationModeEnabled", "Keep hardware acceleration enabled", 1),
    ];

    /// <summary>
    /// Always return false so browser settings are always applied/refreshed.
    /// Registry policies should be idempotent — re-applying is safe and ensures settings stay correct.
    /// </summary>
    public Task<bool> IsInstalledAsync()
    {
        return Task.FromResult(false);
    }

    public Task<bool> InstallAsync(IProgressReporter? progressReporter = null, CancellationToken cancellationToken = default)
    {
        progressReporter?.ReportStatus("Configuring browser privacy settings via Registry Policies...");
        progressReporter?.ReportProgress(5);

        int browserIndex = 0;
        int totalBrowsers = ChromiumBrowsers.Length;
        int successCount = 0;
        var failedBrowsers = new List<string>();

        foreach (var (browserName, policyPath) in ChromiumBrowsers)
        {
            browserIndex++;
            int baseProgress = browserIndex * 90 / totalBrowsers;

            try
            {
                progressReporter?.ReportStatus($"Configuring {browserName}...");

                // Create or open the policy key (HKCU doesn't require admin)
                using var key = Registry.CurrentUser.CreateSubKey(policyPath, writable: true);
                if (key == null)
                {
                    progressReporter?.ReportWarning($"Could not create policy key for {browserName}");
                    failedBrowsers.Add(browserName);
                    continue;
                }

                foreach (var (settingKey, description, value) in PolicySettings)
                {
                    key.SetValue(settingKey, value, RegistryValueKind.DWord);
                }

                successCount++;
                progressReporter?.ReportProgress(baseProgress);
                progressReporter?.ReportStatus($"  ✓ {browserName}: {PolicySettings.Length} policies applied");
            }
            catch (Exception ex)
            {
                progressReporter?.ReportWarning($"Failed to configure {browserName}: {ex.Message}");
                failedBrowsers.Add(browserName);
            }
        }

        // Step 2: Remove browser startup entries from Windows Run registry
        progressReporter?.ReportStatus("Removing browser startup entries...");
        RemoveBrowserStartupEntries(progressReporter);

        // Summary of what was configured
        progressReporter?.ReportProgress(95);
        if (successCount > 0)
        {
            progressReporter?.ReportStatus($"Settings applied to {successCount} browser(s):");
            foreach (var (settingKey, description, value) in PolicySettings)
            {
                var state = value == 1 ? "Enabled" : "Disabled";
                progressReporter?.ReportStatus($"  • {description}: {state}");
            }
        }

        if (failedBrowsers.Count > 0)
        {
            progressReporter?.ReportWarning($"Failed for: {string.Join(", ", failedBrowsers)}");
        }

        progressReporter?.ReportProgress(100);

        if (successCount > 0)
        {
            progressReporter?.ReportSuccess(
                $"Browser privacy settings configured for {successCount}/{totalBrowsers} browsers! " +
                "Restart browsers to apply changes.");
            return Task.FromResult(true);
        }
        else
        {
            progressReporter?.ReportError("Failed to configure browser settings for any browser.");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Remove browser auto-launch entries from HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
    /// to prevent browsers from opening on Windows startup.
    /// </summary>
    private static void RemoveBrowserStartupEntries(IProgressReporter? progressReporter)
    {
        const string runKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        try
        {
            using var runKey = Registry.CurrentUser.OpenSubKey(runKeyPath, writable: true);
            if (runKey == null) return;

            var valueNames = runKey.GetValueNames();
            int removedCount = 0;

            foreach (var valueName in valueNames)
            {
                foreach (var prefix in StartupValuePrefixes)
                {
                    if (valueName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        runKey.DeleteValue(valueName, throwOnMissingValue: false);
                        removedCount++;
                        progressReporter?.ReportStatus($"  ✓ Removed startup entry: {valueName}");
                        break;
                    }
                }
            }

            if (removedCount > 0)
            {
                progressReporter?.ReportStatus($"  Removed {removedCount} browser startup entry/entries");
            }
            else
            {
                progressReporter?.ReportStatus("  No browser startup entries found");
            }
        }
        catch (Exception ex)
        {
            progressReporter?.ReportWarning($"Could not clean startup entries: {ex.Message}");
        }
    }
}