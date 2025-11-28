using System;

namespace DynDNS.Cli.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CliCommandOptionAttribute : Attribute
{
    public string ShortName { get; }
    public string LongName { get; }
    
    public CliCommandOptionAttribute(string option)
    {
        var parts = option.Split('|');
        if (parts.Length == 2)
        {
            ShortName = parts[0].TrimStart('-');
            LongName = parts[1].TrimStart('-');
        }
        else if (option.StartsWith("--"))
        {
            LongName = option.TrimStart('-');
            ShortName = string.Empty;
        }
        else
        {
            ShortName = option.TrimStart('-');
            LongName = string.Empty;
        }
    }
}
