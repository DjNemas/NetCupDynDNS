namespace DynDNS.Cli.Commands;

public abstract class Command<TSettings> where TSettings : CommandSettings, new()
{
    public abstract int Execute(CommandContext context, TSettings settings);
}
