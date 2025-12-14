using DynDNS.Cli.Attributes;
using DynDNS.Cli.Commands;

namespace DynDNS.Commands;

public sealed class UpdateCommandSettings : CommandSettings
{
    [CliDescription("Required - ApiKey of Netcup API.")]
    [CliCommandOption("-k|--apiKey")]
    public string? ApiKey { get; init; }

    [CliDescription("Required - Password for the Netcup API.")]
    [CliCommandOption("-p|--api-password")]
    public string? ApiPassword { get; init; }

    [CliDescription("Required - Customer number of the Netcup.")]
    [CliCommandOption("-n|--customer-number")]
    public int? CustomerNumber { get; init; }

    [CliDescription("Required - Domain that is provided by Netcup.")]
    [CliCommandOption("-d|--domain")]
    public string? Domain { get; init; }

    [CliDescription("Interval in minutes for the updater to re-run again. Keeps the process alive.")]
    [CliCommandOption("-e|--execution-interval-in-minutes")]
    public int? ExecutionIntervalInMinutes { get; init; }

    [CliDescription("In case the execution interval in minutes is set, " +
                 "this flag will drive if the console will live log the remaining time until the next execution. " +
                 "Otherwise it will only print one message with the next execution time.")]
    [CliCommandOption("-t|--ticking-clock")]
    [CliDefaultValue(false)]
    public bool TickingClock { get; init; }

    [CliDescription(
        "List of hosts that will be ignored by the updater. Can be written in the way '-i host1 -i host2 -i host3'.")]
    [CliCommandOption("-i|--ignored-hostname <VALUES>")]
    public string[] IgnoredHosts { get; init; } = [];

    [CliDescription(
        "Save the config file to disk. Be aware, that this file has your credentials in clear text. " +
        "Not setting this flag deletes the configuration file! " +
        "If a configuration file is present, this will take precedence.")]
    [CliCommandOption("-s|--save-config")]
    [CliDefaultValue(false)]
    public bool SaveConfig { get; init; }
}