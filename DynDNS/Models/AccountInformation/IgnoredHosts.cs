using System;
using System.Collections.Generic;
using System.Linq;

namespace DynDNS.Models.AccountInformation;

internal record IgnoredHosts(List<string> Hostnames)
{
    public const string EnvironmentVariableDelimiter = ",";

    /// <summary>
    /// Creates example ignored hosts for configuration file generation.
    /// These are common hosts that users typically want to ignore.
    /// </summary>
    public static List<string> GetExampleHosts() => 
    [
        "*",
        "@", 
        "autoconfig",
        "mail",
        "webmail"
    ];
}