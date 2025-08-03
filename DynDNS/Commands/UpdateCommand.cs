using System;
using System.Linq;
using DynDNS.Models;
using DynDNS.Models.AccountInformation;
using Spectre.Console.Cli;

namespace DynDNS.Commands;

internal class UpdateCommand : Command<UpdateCommandSettings>
{
    public override int Execute(CommandContext context, UpdateCommandSettings updateCommandSettings)
    {
        try
        {
            var configFile = new ConfigFile();

            if (updateCommandSettings.SaveConfig is false) configFile.DeleteFile();

            var accountInformation = GetAccountInformation(updateCommandSettings);

            var updaterService = new UpdaterService();
            var executionIntervalInMinutes = GetExecutionIntervalInMinutes(updateCommandSettings);
            updaterService.RunDnsUpdate(accountInformation, executionIntervalInMinutes,
                updateCommandSettings.TickingClock);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't load account information. Error: {e.Message}, Stacktrace: {e.StackTrace}");
            throw;
        }
    }

    private static int? GetExecutionIntervalInMinutes(UpdateCommandSettings updateCommandSettings)
    {
        int? executionIntervalInMinutes = null;
        var executionIntervalInMinutesFromEnv =
            Environment.GetEnvironmentVariable(EnvironmentVariables.ExecutionIntervalInMinutes);

        if (updateCommandSettings.ExecutionIntervalInMinutes is not null &&
            updateCommandSettings.ExecutionIntervalInMinutes > 0)
            executionIntervalInMinutes = updateCommandSettings.ExecutionIntervalInMinutes;

        else if (string.IsNullOrEmpty(executionIntervalInMinutesFromEnv) is false)
            executionIntervalInMinutes =
                int.Parse(executionIntervalInMinutesFromEnv);

        return executionIntervalInMinutes;
    }

    private AccountInformation GetAccountInformation(UpdateCommandSettings updateCommandSettings)
    {
        var configFile = new ConfigFile();

        if (configFile.DoesConfigFileExist())
            return new ConfigFile().LoadAccountInformation();

        var userCredentials = new UserCredential
        {
            ApiClientKey = updateCommandSettings.ApiKey ??
                           Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiKey),
            ApiClientPW = updateCommandSettings.ApiPassword ??
                          Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiPassword),
            Domain = updateCommandSettings.Domain ??
                     Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupDomain),
            ApiCustomerNumber = updateCommandSettings.CustomerNumber != null
                ? (uint)updateCommandSettings.CustomerNumber
                : uint.Parse(Environment.GetEnvironmentVariable(EnvironmentVariables
                    .NetcupCustomerNumber))
        };

        var ignoredHostsParameter = updateCommandSettings.IgnoredHosts;
        var ignoredHosts = updateCommandSettings.IgnoredHosts == null || ignoredHostsParameter.Length == 0
            ? IgnoredHosts.CreateIgnoredHosts()
            : new IgnoredHosts(ignoredHostsParameter.ToList());

        var accountInformation = new AccountInformation(userCredentials, ignoredHosts);

        if (updateCommandSettings.SaveConfig) configFile.StoreAccountInformation(accountInformation);

        return accountInformation;
    }
}