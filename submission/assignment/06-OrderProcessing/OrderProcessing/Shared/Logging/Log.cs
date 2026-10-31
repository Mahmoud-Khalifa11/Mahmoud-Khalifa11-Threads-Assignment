namespace OrderProcessing.Shared.Logging;

public static class Log
{
    private static readonly object Gate = new();

    public static void Info(string message) => Write(null, message);
    public static void Success(string message) => Write(ConsoleColor.Green, message);
    public static void Error(string message) => Write(ConsoleColor.Red, message);

    private static void Write(ConsoleColor? color, string message)
    {
        string line =
            $"[{DateTime.Now:HH:mm:ss.fff}] [Thread {Environment.CurrentManagedThreadId}] {message}";

        lock (Gate)
        {
            if (color.HasValue)
                Console.ForegroundColor = color.Value;

            try
            {
                Console.WriteLine(line);
            }
            finally
            {
                Console.ResetColor();
            }
        }
  



























  }

}

