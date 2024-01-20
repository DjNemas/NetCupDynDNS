using DynDNS.Models.PublicIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                // Login
                if (request.LoginClient())
                {
                    //Get Records
                    var info = request.GetDNSRecordInfo();
                    List<PublicIPAdresse> currentIPs = request.GetCurrentPublicIP();
                    PublicIPAdresse IPAdress;

                    //Check IPv4
                    IPAdress = currentIPs.FirstOrDefault(x => x.Type == IPType.IPv4);
                    if (IPAdress != null && IPAdress.IP != null)
                    {
                        if (request.DNSRecordDestionationChanged(IPType.IPv4, info, currentIPs))
                        {
                            Console.WriteLine("Destination IPv4 is different! Start Update...");
                            request.UpdateDNSRecord(IPType.IPv4, info.ResponseData, currentIPs);
                        }
                        else
                            Console.WriteLine("Destination IPv4 is same! Skip Update.");
                    }

                    //Check IPv6
                    IPAdress = currentIPs.FirstOrDefault(x => x.Type == IPType.IPv6);
                    if (IPAdress != null && IPAdress.IP != null)
                    {
                        if (request.DNSRecordDestionationChanged(IPType.IPv6, info, currentIPs))
                        {
                            Console.WriteLine("Destination IPv6 is different! Start Update...");
                            request.UpdateDNSRecord(IPType.IPv6, info.ResponseData, currentIPs);
                        }
                        else
                            Console.WriteLine("Destination IPv6 is same! Skip Update.");
                    }
            
                    //Logout
                    request.LogoutClient();
            
                    // Start Timer
                    Console.WriteLine();
                    TimeSpan WaitTime = _WaitTime;
                    while (WaitTime.Ticks > 0)
                    {
                        Console.Write($"\rRetry in {WaitTime}");
                        Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                        WaitTime = WaitTime.Subtract(TimeSpan.FromSeconds(1));
                    }
                    WaitTime = _WaitTime;
                    Console.Clear();
                }
                // If Wrong Login Credential
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
