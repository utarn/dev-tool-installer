namespace DevToolInstaller;

public class MenuProgressReporter : IProgressReporter
{
    private readonly int _startX;
    private readonly int _startY;
    private readonly int _width;

    public MenuProgressReporter(int startX, int startY, int width)
    {
        _startX = startX;
        _startY = startY;
        _width = width;
    }

    public void ReportStatus(string status)
    {
        ConsoleHelper.SetCursorPosition(_startX + 2, _startY + 7);
        ConsoleHelper.ClearCurrentLine();
        ConsoleHelper.WriteInfo($"Status: {status}");
    }

    public void ReportProgress(int percentage)
    {
        ConsoleHelper.SetCursorPosition(_startX + 2, _startY + 5);
        ConsoleHelper.ClearCurrentLine();
        ConsoleHelper.DrawProgressBar(percentage, 100, _width - 4);
    }

    public void ReportProgress(string status, int percentage)
    {
        ReportProgress(percentage);
        ReportStatus(status);
    }

    public void ReportSuccess(string message)
    {
        ConsoleHelper.SetCursorPosition(_startX + 2, _startY + 7);
        ConsoleHelper.ClearCurrentLine();
        ConsoleHelper.WriteSuccess($"Status: {message}");
    }

    public void ReportWarning(string message)
    {
        ConsoleHelper.SetCursorPosition(_startX + 2, _startY + 7);
        ConsoleHelper.ClearCurrentLine();
        ConsoleHelper.WriteWarning($"Status: {message}");
    }

    public void ReportError(string message)
    {
        ConsoleHelper.SetCursorPosition(_startX + 2, _startY + 7);
        ConsoleHelper.ClearCurrentLine();
        ConsoleHelper.WriteError($"Status: {message}");
    }
}