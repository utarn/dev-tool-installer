namespace DevToolInstaller;

public interface IProgressReporter
{
    void ReportStatus(string status);
    void ReportProgress(int percentage);
    void ReportProgress(string status, int percentage);
    void ReportSuccess(string message);
    void ReportWarning(string message);
    void ReportError(string message);
}