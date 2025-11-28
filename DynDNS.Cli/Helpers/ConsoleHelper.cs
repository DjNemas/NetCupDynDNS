using System;

namespace DynDNS.Cli.Helpers;

public static class ConsoleHelper
{
    public static void WriteException(Exception exception, ExceptionFormat format = ExceptionFormat.Default)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        
        switch (format)
        {
            case ExceptionFormat.ShortenEverything:
                Console.WriteLine($"Error: {exception.Message}");
                break;
            case ExceptionFormat.Default:
            default:
                Console.WriteLine($"Exception: {exception.GetType().Name}");
                Console.WriteLine($"Message: {exception.Message}");
                if (exception.StackTrace != null)
                {
                    Console.WriteLine("StackTrace:");
                    Console.WriteLine(exception.StackTrace);
                }
                if (exception.InnerException != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Inner Exception:");
                    WriteException(exception.InnerException, format);
                }
                break;
        }
        
        Console.ForegroundColor = originalColor;
    }
}

public enum ExceptionFormat
{
    Default,
    ShortenEverything
}
