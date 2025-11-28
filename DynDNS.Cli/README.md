# DynDNS.Cli

A lightweight command-line interface framework for .NET 10 applications.

## Purpose

This library provides a simple, dependency-free CLI framework that replaces external dependencies like Spectre.Console. It offers core functionality for command-line argument parsing and command execution.

## Project Structure

```
DynDNS.Cli/
??? Application/
?   ??? CommandApp.cs          # Main application entry point
??? Attributes/
?   ??? CommandOptionAttribute.cs
?   ??? DescriptionAttribute.cs
?   ??? DefaultValueAttribute.cs
??? Commands/
?   ??? Command.cs             # Base command class
?   ??? CommandContext.cs      # Command execution context
?   ??? CommandSettings.cs     # Base settings class
??? Helpers/
    ??? ConsoleHelper.cs       # Console utility methods
```

## Features

- **Command-line argument parsing**: Supports short and long option names (e.g., `-k` and `--api-key`)
- **Type-safe command settings**: Strongly typed command settings with automatic parsing
- **Attribute-based configuration**: Use attributes to define command options and descriptions
- **Array parameter support**: Handle multiple values for a single option
- **Default values**: Specify default values for optional parameters
- **Exception handling**: Configure custom exception handlers for your application
- **Help generation**: Automatic help text generation from attribute metadata

## Namespaces

- **`DynDNS.Cli.Application`**: Main application classes
- **`DynDNS.Cli.Attributes`**: Attribute definitions for CLI configuration
- **`DynDNS.Cli.Commands`**: Base classes for commands and settings
- **`DynDNS.Cli.Helpers`**: Utility helper classes

## Usage Example

```csharp
using DynDNS.Cli.Application;
using DynDNS.Cli.Attributes;
using DynDNS.Cli.Commands;
using DynDNS.Cli.Helpers;

// Define command settings
public class MyCommandSettings : CommandSettings
{
    [Description("API key for authentication")]
    [CommandOption("-k|--api-key")]
    public string? ApiKey { get; init; }

    [Description("Enable verbose output")]
    [CommandOption("-v|--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; init; }
}

// Define command
public class MyCommand : Command<MyCommandSettings>
{
    public override int Execute(CommandContext context, MyCommandSettings settings)
    {
        Console.WriteLine($"API Key: {settings.ApiKey}");
        Console.WriteLine($"Verbose: {settings.Verbose}");
        return 0;
    }
}

// Create and run application
class Program
{
    static int Main(string[] args)
    {
        var app = new CommandApp<MyCommand>();
        
        app.Configure(config =>
        {
            config.SetExceptionHandler(ex =>
            {
                ConsoleHelper.WriteException(ex, ExceptionFormat.ShortenEverything);
            });
        });
        
        return app.Run(args);
    }
}
```

## Command-line Examples

```bash
# Using short option names
MyApp.exe -k "my-api-key" -v

# Using long option names
MyApp.exe --api-key "my-api-key" --verbose

# Array parameters
MyApp.exe -i host1 -i host2 -i host3
```

## License

This library is part of the DynDNS project and follows the same license.

## Notes

- All code comments and strings are in English
- Designed for .NET 10
- No external dependencies
- Lightweight and simple to maintain
- Well-structured with clear separation of concerns
