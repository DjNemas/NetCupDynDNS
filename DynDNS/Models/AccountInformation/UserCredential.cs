using System;

namespace DynDNS.Models.AccountInformation;

internal record UserCredential
{
    public string Domain { get; init; }
    public uint ApiCustomerNumber { get; init; }
    public string ApiClientPW { get; init; }
    public string ApiClientKey { get; init; }
    
    public static UserCredential CreateUserCredentials()
    {
        var customerNumber = 0u;
        try
        {
            customerNumber =
                uint.Parse(Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupCustomerNumber) ?? "0");
        }
        catch (Exception)
        {
            Console.WriteLine(
                $"Couldn't load {EnvironmentVariables.NetcupCustomerNumber} from environment variables. Please set it manually.");
        }

        return new UserCredential
        {
            ApiClientKey = Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiKey) ?? "YourClientKeyHere",
            ApiClientPW = Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupApiPassword) ??
                          "YourClientPWHere",
            ApiCustomerNumber = customerNumber,
            Domain = Environment.GetEnvironmentVariable(EnvironmentVariables.NetcupDomain) ?? "YourDomainHere"
        };
    }
}