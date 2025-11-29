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

        var accountInformation = GetAccountInformation(updateCommandSettings, configFile);

        var executionIntervalInMinutes = GetExecutionIntervalInMinutes(updateCommandSettings);

        var updaterService = new UpdaterService();
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

    private static AccountInformation GetAccountInformation(UpdateCommandSettings updateCommandSettings, ConfigFile configFile)
    {
        var isFirstTimeCreation = !configFile.DoesConfigFileExist();
        var loadedAccountInfo = LoadExistingConfigIfAvailable(configFile, isFirstTimeCreation);

        var credentials = BuildUserCredentials(updateCommandSettings, loadedAccountInfo);
        var ignoredHosts = GetIgnoredHosts(updateCommandSettings, loadedAccountInfo);

        if (updateCommandSettings.SaveConfig && isFirstTimeCreation)
        {
            return HandleFirstTimeConfigCreation(configFile, credentials);
        }

        ValidateRequiredCredentials(credentials);

        var accountInformation = new AccountInformation(credentials.Final!, ignoredHosts);

        if (updateCommandSettings.SaveConfig)
        {
            SaveConfigFile(configFile, credentials.Final!, ignoredHosts);
        }

        return accountInformation;
    }

    private static AccountInformation? LoadExistingConfigIfAvailable(ConfigFile configFile, bool isFirstTimeCreation)
    {
        return isFirstTimeCreation ? null : configFile.LoadAccountInformation();
    }

    private static (string? ApiKey, string? ApiPassword, string? Domain, uint? CustomerNumber, UserCredential? Final) 
        BuildUserCredentials(UpdateCommandSettings updateCommandSettings, AccountInformation? loadedAccountInfo)
    {
        var apiKey = GetValueWithPriority(
            updateCommandSettings.ApiKey,
            loadedAccountInfo?.Credentials.ApiClientKey,
            Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiKey),
            null);

        var apiPassword = GetValueWithPriority(
            updateCommandSettings.ApiPassword,
            loadedAccountInfo?.Credentials.ApiClientPW,
            Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiPassword),
            null);

        var domain = GetValueWithPriority(
            updateCommandSettings.Domain,
            loadedAccountInfo?.Credentials.Domain,
            Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupDomain),
            null);

        var customerNumber = GetCustomerNumberWithPriority(
            updateCommandSettings.CustomerNumber,
            loadedAccountInfo?.Credentials.ApiCustomerNumber,
            Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupCustomerNumber),
            null);

        UserCredential? finalCredentials = null;
        if (apiKey != null && apiPassword != null && domain != null && customerNumber != null)
        {
            finalCredentials = new UserCredential
            {
                ApiClientKey = apiKey,
                ApiClientPW = apiPassword,
                Domain = domain,
                ApiCustomerNumber = customerNumber.Value
            };
        }

        return (apiKey, apiPassword, domain, customerNumber, finalCredentials);
    }

    private static AccountInformation HandleFirstTimeConfigCreation(
        ConfigFile configFile,
        (string? ApiKey, string? ApiPassword, string? Domain, uint? CustomerNumber, UserCredential? Final) credentials)
    {
        var placeholderCredentials = new UserCredential
        {
            ApiClientKey = credentials.ApiKey ?? "your_api_key_here",
            ApiClientPW = credentials.ApiPassword ?? "your_api_password_here",
            Domain = credentials.Domain ?? "example.com",
            ApiCustomerNumber = credentials.CustomerNumber ?? 123456
        };

        var exampleIgnoredHosts = new IgnoredHosts(IgnoredHosts.GetExampleHosts());
        var accountInfoToSave = new AccountInformation(placeholderCredentials, exampleIgnoredHosts);
        configFile.StoreAccountInformation(accountInfoToSave);

        Console.WriteLine($"Config file created: {ConfigFile.FileName}");
        Console.WriteLine("Please edit the file with your actual credentials and run again.");
        Console.WriteLine();

        if (credentials.Final == null)
        {
            Console.WriteLine("Configuration file created. Please fill in your credentials and run the application again.");
            credentials.Final = placeholderCredentials;
        }

        return new AccountInformation(credentials.Final, exampleIgnoredHosts);
    }

    private static void ValidateRequiredCredentials(
        (string? ApiKey, string? ApiPassword, string? Domain, uint? CustomerNumber, UserCredential? Final) credentials)
    {
        if (credentials.ApiKey == null)
            throw new InvalidOperationException(
                "API Key is required. Provide it via CLI parameter, config file, or environment variable.");
        
        if (credentials.ApiPassword == null)
            throw new InvalidOperationException(
                "API Password is required. Provide it via CLI parameter, config file, or environment variable.");
        
        if (credentials.Domain == null)
            throw new InvalidOperationException(
                "Domain is required. Provide it via CLI parameter, config file, or environment variable.");
        
        if (credentials.CustomerNumber == null)
            throw new InvalidOperationException(
                "Customer Number is required. Provide it via CLI parameter, config file, or environment variable.");
    }

    private static void SaveConfigFile(ConfigFile configFile, UserCredential credentials, IgnoredHosts ignoredHosts)
    {
        var ignoredHostsForConfig = ignoredHosts.Hostnames.Count == 0
            ? new IgnoredHosts(IgnoredHosts.GetExampleHosts())
            : ignoredHosts;

        var accountInfoToSave = new AccountInformation(credentials, ignoredHostsForConfig);
        configFile.StoreAccountInformation(accountInfoToSave);

        if (ignoredHosts.Hostnames.Count == 0)
        {
            Console.WriteLine($"Config file updated with example ignored hosts. Edit {ConfigFile.FileName} to customize.");
        }
    }

    private static string? GetValueWithPriority(string? cliValue, string? configValue, string? envValue, string? parameterName)
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

        return null;
    }

    private static uint? GetCustomerNumberWithPriority(int? cliValue, uint? configValue, string? envValue, string? parameterName)
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

        return null;
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