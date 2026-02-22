using System;
using System.IO;

public class Logger
{
    private readonly string logFilePath = "instagram_post.log";

    public void Log(string message)
    {
        string logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(logEntry);
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
    }
}