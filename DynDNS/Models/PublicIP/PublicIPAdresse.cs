namespace DynDNS.Models.PublicIP
{
    public class PublicIPAdresse
    {
        public string IP { get; set; } = string.Empty;
        public IPType Type { get; set; }
    }

    public enum IPType
    {
        IPv6,
        IPv4,
    }
}
