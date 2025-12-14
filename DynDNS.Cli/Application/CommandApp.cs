using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynDNS.Cli.Attributes;
using DynDNS.Cli.Commands;

namespace DynDNS.Cli.Application;

public class CommandApp<TCommand> where TCommand : class, new()
{
    private Action<Exception>? _exceptionHandler;
    
    public void Configure(Action<CommandAppConfiguration> configure)
    {
        var config = new CommandAppConfiguration();
        configure(config);
        _exceptionHandler = config.ExceptionHandler;
    }
    
    public int Run(string[] args)
    {
        try
        {
            var commandType = typeof(TCommand);
            var executeMethod = commandType.GetMethod("Execute");
            
            if (executeMethod == null)
            {
                Console.WriteLine("Error: Command does not have an Execute method.");
                return 1;
            }
            
            var settingsType = executeMethod.GetParameters()[1].ParameterType;
            var settings = Activator.CreateInstance(settingsType);
            
            if (settings == null)
            {
                Console.WriteLine("Error: Could not create settings instance.");
                return 1;
            }
            
            // Parse arguments
            if (!ParseArguments(args, settings))
            {
                PrintHelp(settingsType);
                return 1;
            }
            
            var command = Activator.CreateInstance(commandType);
            var context = new CommandContext();
            
            var result = executeMethod.Invoke(command, new[] { context, settings });
            return result is int exitCode ? exitCode : 0;
        }
        catch (Exception ex)
        {
            if (_exceptionHandler != null)
            {
                _exceptionHandler(ex.InnerException ?? ex);
                return 1;
            }
            throw;
        }
    }
    
    private bool ParseArguments(string[] args, object settings)
    {
        var properties = settings.GetType().GetProperties();
        var propertyMap = new Dictionary<string, PropertyInfo>();
        var arrayProperties = new Dictionary<string, List<string>>();
        
        // Build property map
        foreach (var prop in properties)
        {
            var optionAttr = prop.GetCustomAttribute<CliCommandOptionAttribute>();
            if (optionAttr != null)
            {
                if (!string.IsNullOrEmpty(optionAttr.ShortName))
                    propertyMap[optionAttr.ShortName] = prop;
                if (!string.IsNullOrEmpty(optionAttr.LongName))
                    propertyMap[optionAttr.LongName] = prop;
            }
        }
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("-"))
                continue;
                
            var key = arg.TrimStart('-');
            
            if (!propertyMap.TryGetValue(key, out var property))
            {
                Console.WriteLine($"Unknown option: {arg}");
                return false;
            }
            
            var propertyType = property.PropertyType;
            
            // Handle boolean flags
            if (propertyType == typeof(bool))
            {
                property.SetValue(settings, true);
                continue;
            }
            
            // Handle array types
            if (propertyType.IsArray)
            {
                if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    i++;
                    if (!arrayProperties.ContainsKey(property.Name))
                        arrayProperties[property.Name] = new List<string>();
                    arrayProperties[property.Name].Add(args[i]);
                }
                continue;
            }
            
            // Get next value
            if (i + 1 >= args.Length || args[i + 1].StartsWith("-"))
            {
                Console.WriteLine($"Option {arg} requires a value.");
                return false;
            }
            
            i++;
            var value = args[i];
            
            // Set property value
            try
            {
                if (propertyType == typeof(string))
                {
                    property.SetValue(settings, value);
                }
                else if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    property.SetValue(settings, int.Parse(value));
                }
                else if (propertyType == typeof(uint) || propertyType == typeof(uint?))
                {
                    property.SetValue(settings, uint.Parse(value));
                }
                else if (propertyType == typeof(double) || propertyType == typeof(double?))
                {
                    property.SetValue(settings, double.Parse(value));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing value for {arg}: {ex.Message}");
                return false;
            }
        }
        
        // Set array properties
        foreach (var kvp in arrayProperties)
        {
            var property = properties.First(p => p.Name == kvp.Key);
            var elementType = property.PropertyType.GetElementType();
            
            if (elementType == typeof(string))
            {
                property.SetValue(settings, kvp.Value.ToArray());
            }
        }
        
        // Set default values
        foreach (var prop in properties)
        {
            var defaultValueAttr = prop.GetCustomAttribute<CliDefaultValueAttribute>();
            if (defaultValueAttr != null && prop.GetValue(settings) == null)
            {
                prop.SetValue(settings, defaultValueAttr.DefaultValue);
            }
        }
        
        return true;
    }
    
    private void PrintHelp(Type settingsType)
    {
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine($"    {AppDomain.CurrentDomain.FriendlyName} [OPTIONS]");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        
        var properties = settingsType.GetProperties();
        foreach (var prop in properties)
        {
            var optionAttr = prop.GetCustomAttribute<CliCommandOptionAttribute>();
            var descAttr = prop.GetCustomAttribute<CliDescriptionAttribute>();
            
            if (optionAttr == null)
                continue;
                
            var shortName = !string.IsNullOrEmpty(optionAttr.ShortName) ? $"-{optionAttr.ShortName}" : "";
            var longName = !string.IsNullOrEmpty(optionAttr.LongName) ? $"--{optionAttr.LongName}" : "";
            var options = string.IsNullOrEmpty(shortName) ? longName : 
                         string.IsNullOrEmpty(longName) ? shortName : 
                         $"{shortName}, {longName}";
            
            Console.WriteLine($"    {options,-40}");
            
            if (descAttr != null)
            {
                var description = descAttr.Description.Replace("[red]", "").Replace("[/]", "");
                Console.WriteLine($"        {description}");
            }
            
            Console.WriteLine();
        }
    }
}

public class CommandAppConfiguration
{
    internal Action<Exception>? ExceptionHandler { get; private set; }
    
    public void SetExceptionHandler(Action<Exception> handler)
    {
        ExceptionHandler = handler;
    }
}
