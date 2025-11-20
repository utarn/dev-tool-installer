namespace DevToolInstaller;

public static class ConsoleHelper
{
    private static readonly object _lock = new();

    public static void WriteHeader(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public static void WriteInfo(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public static void WriteSuccess(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public static void WriteWarning(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public static void WriteError(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public static void WriteProgress(string message)
    {
        lock (_lock)
        {
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1));
            Console.Write("\r");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(message);
            Console.ResetColor();
        }
    }

    public static void ClearCurrentLine()
    {
        lock (_lock)
        {
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1));
            Console.Write("\r");
        }
    }

    public static void DrawMenuBorder(int width, int height)
    {
        lock (_lock)
        {
            Console.WriteLine("+" + new string('-', width - 2) + "+");
            for (int i = 0; i < height - 2; i++)
            {
                Console.WriteLine("|" + new string(' ', width - 2) + "|");
            }
            Console.WriteLine("+" + new string('-', width - 2) + "+");
        }
    }

    public static void DrawMenuItem(string text, bool isSelected, bool isInstalled = false)
    {
        lock (_lock)
        {
            if (isSelected)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"> {text}");
                if (isInstalled)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" (installed)");
                }
                Console.ResetColor();
            }
            else
            {
                Console.Write($"  {text}");
                if (isInstalled)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" (installed)");
                    Console.ResetColor();
                }
            }
            Console.WriteLine();
        }
    }

    public static void DrawProgressBar(int current, int total, int width = 50)
    {
        lock (_lock)
        {
            var percentage = total > 0 ? (int)((double)current / total * 100) : 0;
            var filledWidth = (int)((double)current / total * width);
            var emptyWidth = width - filledWidth;
            
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('#', filledWidth));
            Console.ResetColor();
            Console.Write(new string(' ', emptyWidth));
            Console.Write($"] {percentage}%");
        }
    }

    public static void DrawStatusIndicator(bool isInstalled)
    {
        lock (_lock)
        {
            if (isInstalled)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("(installed)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("↓");
            }
            Console.ResetColor();
        }
    }

    public static void ClearMenuArea(int startLine, int height)
    {
        lock (_lock)
        {
            var currentTop = Console.CursorTop;
            for (int i = 0; i < height; i++)
            {
                Console.SetCursorPosition(0, startLine + i);
                Console.Write(new string(' ', Console.WindowWidth));
            }
            Console.SetCursorPosition(0, startLine);
        }
    }

    public static void ShowHelpText(string[] helpItems)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var item in helpItems)
            {
                Console.WriteLine(item);
            }
            Console.ResetColor();
        }
    }

    public static void SetCursorPosition(int x, int y)
    {
        lock (_lock)
        {
            try
            {
                Console.SetCursorPosition(x, y);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Handle console resizing issues
            }
        }
    }

    public static void DrawBorderedBox(int x, int y, int width, int height, string? title = null)
    {
        lock (_lock)
        {
            var originalCursorTop = Console.CursorTop;
            var originalCursorLeft = Console.CursorLeft;
            
            // Save current position and move to start
            SetCursorPosition(x, y);
            
            // Top border
            Console.Write("+");
            if (!string.IsNullOrEmpty(title))
            {
                var titlePadding = Math.Max(0, width - title.Length - 4);
                var leftPadding = titlePadding / 2;
                var rightPadding = titlePadding - leftPadding;
                Console.Write(new string('-', leftPadding));
                Console.Write($" {title} ");
                Console.Write(new string('-', rightPadding));
            }
            else
            {
                Console.Write(new string('-', width - 2));
            }
            Console.WriteLine("+");
            
            // Middle section
            for (int i = 1; i < height - 1; i++)
            {
                SetCursorPosition(x, y + i);
                Console.WriteLine("|" + new string(' ', width - 2) + "|");
            }
            
            // Bottom border
            SetCursorPosition(x, y + height - 1);
            Console.WriteLine("+" + new string('-', width - 2) + "+");
            
            // Restore cursor position
            SetCursorPosition(originalCursorLeft, originalCursorTop);
        }
    }

    public static void ClearScreen()
    {
        lock (_lock)
        {
            Console.Clear();
        }
    }
}