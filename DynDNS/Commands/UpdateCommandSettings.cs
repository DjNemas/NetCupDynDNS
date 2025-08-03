using System.ComponentModel;
using Spectre.Console.Cli;

namespace DynDNS.Commands;

public sealed class UpdateCommandSettings : CommandSettings
{
    [Description("[red]Required[/] - ApiKey of Netcup API.")]
    [CommandOption("-k|--apiKey")]
    public string? ApiKey { get; init; }

    [Description("[red]Required[/] - Password for the Netcup API.")]
    [CommandOption("-p|--api-password")]
    public string? ApiPassword { get; init; }

    [Description("[red]Required[/] - Customer number of the Netcup.")]
    [CommandOption("-n|--customer-number")]
    public int? CustomerNumber { get; init; }

    [Description("[red]Required[/] - Domain that is provided by Netcup.")]
    [CommandOption("-d|--domain")]
    public string? Domain { get; init; }

    [Description("Interval in minutes for the updater to re-run again. Keeps the process alive.")]
    [CommandOption("-e|--execution-interval-in-minutes")]
    public int? ExecutionIntervalInMinutes { get; init; }

    [Description("In case the execution interval in minutes is set, " +
                 "this flag will drive if the console will live log the remaining time until the next execution. " +
                 "Otherwise it will only print one message with the next execution time.")]
    [CommandOption("-t|--ticking-clock")]
    [DefaultValue(false)]
    public bool TickingClock { get; init; }

    [Description(
        "List of hosts that will be ignored by the updater. Can be written in the way '-i host1 -i host2 -i host3'.")]
    [CommandOption("-i|--ignored-hostname <VALUES>")]
    public string[] IgnoredHosts { get; init; }

    [Description(
        "Save the config file to disk. Be aware, that this file has your credentials in clear text. " +
        "Not setting this flag deletes the configuration file! " +
        "If a configuration file is present, this will take precedence.")]
    [CommandOption("-s|--save-config")]
    [DefaultValue(false)]
    public bool SaveConfig { get; init; }
}