using System;

namespace DynDNS.Models.AccountInformation;

internal record UserCredential
{
    public required string Domain { get; init; }
    public uint ApiCustomerNumber { get; init; }
    public required string ApiClientPW { get; init; }
    public required string ApiClientKey { get; init; }
    
    public static UserCredential LoadFromEnvironmentOrDefault()
    {
        var customerNumber = 0u;
        try
        {
            customerNumber = uint.Parse(Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupCustomerNumber) ?? "0");
        }
        catch (Exception)
        {
            Console.WriteLine($"Couldn't load {EnvironmentVariables.NetcupCustomerNumber} from environment variables. Please set it manually.");
        }

        return new UserCredential
        {
            ApiClientKey = Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiKey) ?? "YourClientKeyHere",
            ApiClientPW = Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiPassword) ?? "YourClientPWHere",
            ApiCustomerNumber = customerNumber,
            Domain = Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupDomain) ?? "YourDomainHere"
        };
    }
}