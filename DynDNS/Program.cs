using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynDNS.Models;
using DynDNS.Models.AccountInformation;
using DynDNS.Models.Actions;
using DynDNS.Models.DNSInfoResponse;
using DynDNS.Models.PublicIP;

namespace DynDNS;

internal class Program
{
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromMinutes(
        int.Parse(Environment.GetEnvironmentVariable(EnvironmentVariables.ExecutionIntervalInMinutes) ?? "60"));

    private const int ShutdownTimeInSeconds = 30;
        
    static void Main()
    {
        var accountInformation = new AccountInformationFile().CreateIfNotExist().LoadAccountInformation();
        var request = new WebRequest(accountInformation.Credentials);
            
        while (true)
        {
            if (request.LoginClient())
            {
                var info = GetDnsRecords(request, accountInformation, out var currentIPs);

                UpdateDnsRecords(currentIPs, request, info, IPType.IPv4);

                UpdateDnsRecords(currentIPs, request, info, IPType.IPv6);
                    
                request.LogoutClient();

                StartWaitTimer();
                Console.Clear();
            }
            else
            {
                Console.WriteLine($"Closing application in {ShutdownTimeInSeconds} seconds.");
                Task.Delay(TimeSpan.FromSeconds(ShutdownTimeInSeconds));
                break;
            }
        }
    }

    private static void StartWaitTimer()
    {
        Console.WriteLine($"Next execution in {ExecutionInterval} minutes.");
        var waitTime = ExecutionInterval;
        while (waitTime.Ticks > 0)
        {
            Console.Write($"\rRetry in {waitTime}");
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            waitTime = waitTime.Subtract(TimeSpan.FromSeconds(1));
        }
    }

    private static void UpdateDnsRecords(List<PublicIPAdresse> currentIPs, WebRequest request,
        ResponseMessage<DNSRecords> info, IPType ipType)
    {
        var ipAddress = currentIPs.FirstOrDefault(x => x.Type == ipType);
            
        if (ipAddress is not { IP: not null }) return;
            
        if (request.DNSRecordDestionationChanged(ipType, info, currentIPs))
        {
            Console.WriteLine($"Destination {ipType.ToString()} is different! Start Update...");
            request.UpdateDNSRecord(ipType, info.ResponseData, currentIPs);
        }
        else
        {
            Console.WriteLine($"Destination {ipType.ToString()} is same! Skip Update.");
        }
    }

    private static ResponseMessage<DNSRecords> GetDnsRecords(WebRequest request,
        AccountInformation accountInformation,
        out List<PublicIPAdresse> currentIPs)
    {
        var info = request.GetDNSRecordInfo();
        info.ResponseData.RemoveHosts(accountInformation.IgnoredHosts.Hostnames);
        currentIPs = request.GetCurrentPublicIP();
        return info;
    }
}