using DynDNS.Commands;
using DynDNS.Cli.Application;
using DynDNS.Cli.Helpers;

namespace DynDNS;

internal class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp<UpdateCommand>();

        app.Configure(config =>
        {
            config.SetExceptionHandler((ex) =>
            {
                ConsoleHelper.WriteException(ex, ExceptionFormat.ShortenEverything);
            });
        });
        return app.Run(args);
    }
}