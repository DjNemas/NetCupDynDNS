using DynDNS.Cli.Commands;
using DynDNS.Models;
using DynDNS.Models.AccountInformation;
using System;
using System.Linq;

namespace DynDNS.Commands;

internal class UpdateCommand : Command<UpdateCommandSettings>
{
    private const int _defaultExecutionIntervalMinutes = 60;

    public override int Execute(CommandContext context, UpdateCommandSettings updateCommandSettings)
    {
        var configFile = new ConfigFile();

        if (updateCommandSettings.SaveConfig is false)
            configFile.DeleteFile();

        var accountInformation = GetAccountInformation(updateCommandSettings);

        var updaterService = new UpdaterService();
        var executionIntervalInMinutes = GetExecutionIntervalInMinutes(updateCommandSettings);
        updaterService.RunDnsUpdate(accountInformation, executionIntervalInMinutes,
            updateCommandSettings.TickingClock);

        return 0;
    }

    private static int GetExecutionIntervalInMinutes(UpdateCommandSettings updateCommandSettings)
    {
        int executionIntervalInMinutes = _defaultExecutionIntervalMinutes;
        var executionIntervalInMinutesFromEnv = Environment.GetEnvironmentVariable(EnvironmentVariables.ExecutionIntervalInMinutes);

        // Setting File First Priority
        if (updateCommandSettings.ExecutionIntervalInMinutes is not null &&
            updateCommandSettings.ExecutionIntervalInMinutes >= 0)
        {
            executionIntervalInMinutes = (int)updateCommandSettings.ExecutionIntervalInMinutes;
        }
        // Environment Variable Second Priority
        else if (!string.IsNullOrEmpty(executionIntervalInMinutesFromEnv))
        {
            if(!int.TryParse(executionIntervalInMinutesFromEnv, out executionIntervalInMinutes))
                executionIntervalInMinutes = _defaultExecutionIntervalMinutes;
        }

        return executionIntervalInMinutes;
    }

    private static AccountInformation GetAccountInformation(UpdateCommandSettings updateCommandSettings)
    {
        var configFile = new ConfigFile();

        // Try to load from config file (Priority 2)
        AccountInformation? loadedAccountInfo = null;
        if (configFile.DoesConfigFileExist())
        {
            loadedAccountInfo = configFile.LoadAccountInformation();
        }

        // Build UserCredential with priority: CLI (1) > ConfigFile (2) > Environment (3)
        var userCredentials = new UserCredential
        {
            ApiClientKey = GetValueWithPriority(
                updateCommandSettings.ApiKey,
                loadedAccountInfo?.Credentials.ApiClientKey,
                Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiKey),
                "API Key"),

            ApiClientPW = GetValueWithPriority(
                updateCommandSettings.ApiPassword,
                loadedAccountInfo?.Credentials.ApiClientPW,
                Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiPassword),
                "API Password"),

            Domain = GetValueWithPriority(
                updateCommandSettings.Domain,
                loadedAccountInfo?.Credentials.Domain,
                Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupDomain),
                "Domain"),

            ApiCustomerNumber = GetCustomerNumberWithPriority(
                updateCommandSettings.CustomerNumber,
                loadedAccountInfo?.Credentials.ApiCustomerNumber,
                Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupCustomerNumber))
        };

        // Handle ignored hosts
        var ignoredHosts = GetIgnoredHosts(updateCommandSettings, loadedAccountInfo);

        var accountInformation = new AccountInformation(userCredentials, ignoredHosts);

        if (updateCommandSettings.SaveConfig)
        {
            // If this is the first time creating the config and no ignored hosts were specified, add examples
            var isFirstTimeCreation = !configFile.DoesConfigFileExist();
            var ignoredHostsForConfig = isFirstTimeCreation && ignoredHosts.Hostnames.Count == 0
                ? new IgnoredHosts(IgnoredHosts.GetExampleHosts())
                : ignoredHosts;
            
            var accountInfoToSave = new AccountInformation(userCredentials, ignoredHostsForConfig);
            configFile.StoreAccountInformation(accountInfoToSave);
            
            if (isFirstTimeCreation && ignoredHosts.Hostnames.Count == 0)
            {
                Console.WriteLine($"Config file created with example ignored hosts. Edit {ConfigFile.FileName} to customize.");
            }
        }

        return accountInformation;
    }

    private static string GetValueWithPriority(string? cliValue, string? configValue, string? envValue, string parameterName)
    {
        // Priority 1: CLI parameter
        if (!string.IsNullOrEmpty(cliValue))
            return cliValue;

        // Priority 2: Config file
        if (!string.IsNullOrEmpty(configValue))
            return configValue;

        // Priority 3: Environment variable
        if (!string.IsNullOrEmpty(envValue))
            return envValue;

        throw new InvalidOperationException($"{parameterName} is required. Provide it via CLI parameter, config file, or environment variable.");
    }

    private static uint GetCustomerNumberWithPriority(int? cliValue, uint? configValue, string? envValue)
    {
        // Priority 1: CLI parameter
        if (cliValue.HasValue && cliValue.Value > 0)
            return (uint)cliValue.Value;

        // Priority 2: Config file
        if (configValue.HasValue && configValue.Value > 0)
            return configValue.Value;

        // Priority 3: Environment variable
        if (!string.IsNullOrEmpty(envValue) && uint.TryParse(envValue, out var parsedValue) && parsedValue > 0)
            return parsedValue;

        throw new InvalidOperationException("Customer Number is required. Provide it via CLI parameter, config file, or environment variable.");
    }

    private static IgnoredHosts GetIgnoredHosts(UpdateCommandSettings updateCommandSettings, AccountInformation? loadedAccountInfo)
    {
        // Priority 1: CLI parameter
        if (updateCommandSettings.IgnoredHosts is { Length: > 0 })
            return new IgnoredHosts(updateCommandSettings.IgnoredHosts.ToList());

        // Priority 2: Config file
        if (loadedAccountInfo?.IgnoredHosts is { Hostnames.Count: > 0 })
            return loadedAccountInfo.IgnoredHosts;

        // Priority 3: Environment variable (if set)
        var envIgnoredHosts = Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupIgnoredHosts);
        if (!string.IsNullOrEmpty(envIgnoredHosts))
        {
            var hostnames = envIgnoredHosts.Split(IgnoredHosts.EnvironmentVariableDelimiter).ToList();
            return new IgnoredHosts(hostnames);
        }

        // No hosts to ignore
        return new IgnoredHosts([]);
    }
}