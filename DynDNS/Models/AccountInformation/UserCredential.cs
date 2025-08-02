using System;

namespace DynDNS.Models.AccountInformation;

internal class UserCredential
{
    public string Domain { get; set; }
    public uint ApiCustomerNumber { get; set; }
    public string ApiClientPW { get; set; }
    public string ApiClientKey { get; set; }

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