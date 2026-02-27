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
    public string Description => "Configure browsers: ask download location, disable background/analytics/startup boost/auto-update";
    public List<string> Dependencies => new();

    private static readonly (string Name, string PolicyPath)[] ChromiumBrowsers =
    [
        ("Google Chrome", @"SOFTWARE\Policies\Google\Chrome"),
        ("Microsoft Edge", @"SOFTWARE\Policies\Microsoft\Edge"),
        ("Brave", @"SOFTWARE\Policies\BraveSoftware\Brave"),
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

    public Task<bool> IsInstalledAsync()
    {
        try
        {
            int configuredCount = 0;
            int checkedCount = 0;

            foreach (var (name, policyPath) in ChromiumBrowsers)
            {
                using var key = Registry.CurrentUser.OpenSubKey(policyPath);
                if (key == null) continue;

                checkedCount++;
                bool allSet = true;

                foreach (var (settingKey, _, expectedValue) in PolicySettings)
                {
                    var val = key.GetValue(settingKey);
                    if (val is not int intVal || intVal != expectedValue)
                    {
                        allSet = false;
                        break;
                    }
                }

                if (allSet) configuredCount++;
            }

            // Consider "installed" if we found at least one browser policy key and all are configured
            if (checkedCount > 0 && configuredCount == checkedCount)
            {
                return Task.FromResult(true);
            }
        }
        catch
        {
            // If we can't read registry, assume not configured
        }

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
}