using DynDNS.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DynDNS;

internal class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp<UpdateCommand>();

        app.Configure(config =>
        {
            config.SetExceptionHandler((ex, resolver) =>
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            });
        });
        return app.Run(args);
        
    }
}