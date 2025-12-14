namespace DynDNS.Cli.Commands;

public class CommandContext
{
    public string[] RemainingArguments { get; internal set; }
    
    public CommandContext()
    {
        RemainingArguments = [];
    }
}
