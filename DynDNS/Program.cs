using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DynDNS
{
    internal class Program
    {
        private static TimeSpan _WaitTime = TimeSpan.FromMinutes(1);
        static void Main()
        {
            CredentialFile credential = new CredentialFile().CreateIfNotExist();
            WebRequest request = new WebRequest(credential.LoadCredential());

            while (true)
            {
                if (request.LoginClient())
                {
                    var info = request.GetDNSRecordInfo();
                    if (request.DNSRecordDestionationChanged(info))
                    {
                        Console.WriteLine("Destination IP is different! Start Update...");
                        request.UpdateDNSRecord(info.ResponseData);
                    }
                    else
                        Console.WriteLine("Destination IP is same! Skip Update.");
                    request.LogoutClient();

                    Console.WriteLine();
                    while (_WaitTime.Ticks > 0)
                    {
                        Console.WriteLine($"Retry in {_WaitTime}");
                        Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                        _WaitTime = _WaitTime.Subtract(TimeSpan.FromSeconds(1));
                    }
                    _WaitTime = TimeSpan.FromMinutes(1);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Press any key to close this Application...");
                    Console.ReadKey();
                    break;
                }
            }
        }
    }
}
