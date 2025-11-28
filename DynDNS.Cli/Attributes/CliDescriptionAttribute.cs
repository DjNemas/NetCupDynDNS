using System;

namespace DynDNS.Cli.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CliDescriptionAttribute : Attribute
{
    public string Description { get; }
    
    public CliDescriptionAttribute(string description)
    {
        Description = description;
    }
}
