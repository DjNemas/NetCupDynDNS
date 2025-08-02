namespace DynDNS.Models.AccountInformation;

internal record AccountInformation(UserCredential Credentials, IgnoredHosts IgnoredHosts)
{
    public AccountInformation() : this(UserCredential.CreateUserCredentials(), IgnoredHosts.CreateIgnoredHosts())
    {
    }
};