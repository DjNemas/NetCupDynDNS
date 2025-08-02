using System;
using System.Collections.Generic;
using System.Linq;

namespace DynDNS.Models.AccountInformation;

internal record IgnoredHosts(List<string> Hostnames)
{
    private static readonly List<string> DefaultHostnames =
    [
        "*", "@", "autoconfig", "db", "google", "key1._domainkey", "key2._domainkey", "mail", "webmail"
    ];

    private const string EnvironmentVariableDelimiter = ",";

    public static IgnoredHosts CreateIgnoredHosts()
    {
        var ignoredHostnames = DefaultHostnames;
        try
        {
            var hostNamesEnvironmentVariable =
                Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupIgnoredHosts);

            if (string.IsNullOrEmpty(hostNamesEnvironmentVariable))
                throw new ArgumentNullException(nameof(hostNamesEnvironmentVariable));

            ignoredHostnames = hostNamesEnvironmentVariable.Split(EnvironmentVariableDelimiter).ToList();
        }
        catch (Exception)
        { 
            Console.WriteLine(
                $"Couldn't read environment variable {EnvironmentVariables.NetcupIgnoredHosts}. \n" +
                $"Loading default hostnames \"{string.Join(",", DefaultHostnames)}\"!");
        }

        IgnoredHosts ignoredHosts = new(ignoredHostnames);
        return ignoredHosts;
    }
}