using System;

namespace DynDNS.Cli.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CliDefaultValueAttribute : Attribute
{
    public object DefaultValue { get; }
    
    public CliDefaultValueAttribute(object defaultValue)
    {
        DefaultValue = defaultValue;
    }
}
