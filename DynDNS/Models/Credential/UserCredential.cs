namespace DynDNS.Models.Credential
{
    public class UserCredential
    {
        public string Domain { get; set; }
        public uint ApiCustomerNumber { get; set; }
        public string ApiClientPW { get; set; }
        public string ApiClientKey { get; set; }
    }
}
