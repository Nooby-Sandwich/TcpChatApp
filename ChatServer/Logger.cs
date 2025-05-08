using System;
using System.IO;

public static class Logger
{
    private static readonly string LogPath = "chatserver.log";

    public static void Log(string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        Console.WriteLine(line);
        File.AppendAllText(LogPath, line + Environment.NewLine);
    }

    public static void LogPrivateMessage(string sender, string recipient, string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] PM: {sender} -> {recipient}: {message}";
        Console.WriteLine(line);
        File.AppendAllText(LogPath, line + Environment.NewLine);
    }
}