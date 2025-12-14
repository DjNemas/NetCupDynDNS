using DynDNS.Models.AccountInformation;
using DynDNS.Models.Actions;
using DynDNS.Models.DNSInfoResponse;
using DynDNS.Models.PublicIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynDNS;

internal class UpdaterService
{
    private const int _shutdownTimeInSeconds = 30;

    public void RunDnsUpdate(AccountInformation accountInformation, int executionIntervalInMinutes, bool tickingClock)
    {
        var request = new WebRequest(accountInformation.Credentials);
        var shouldBeKeptAlive = executionIntervalInMinutes is > 0;

        do
        {
            if (request.LoginClient())
            {
                var info = GetDnsRecords(request, accountInformation, out var currentIPs);

                if (info != null)
                {
                    UpdateDnsRecords(currentIPs, request, info, IPType.IPv4);

                    UpdateDnsRecords(currentIPs, request, info, IPType.IPv6);
                }

                request.LogoutClient();

                if (shouldBeKeptAlive)
                {
                    StartWaitTimer(tickingClock, executionIntervalInMinutes);
                    Console.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Closing application in {_shutdownTimeInSeconds} seconds.");
                Task.Delay(TimeSpan.FromSeconds(_shutdownTimeInSeconds)).Wait();
                shouldBeKeptAlive = false;
            }
        } while (shouldBeKeptAlive);
    }

    private static void StartWaitTimer(bool tickingClock, int executionIntervalInMinutes)
    {
        var executionInterval = TimeSpan.FromMinutes(executionIntervalInMinutes);

        if (tickingClock)
        {
            ShowTickingCountdown(executionInterval);
        }
        else
        {
            ShowStaticWaitMessage(executionInterval);
            Task.Delay(executionInterval).Wait();
        }
    }

    private static void ShowTickingCountdown(TimeSpan executionInterval)
    {
        var endTime = DateTime.Now.Add(executionInterval);
        
        while (DateTime.Now < endTime)
        {
            var remaining = endTime - DateTime.Now;
            Console.Write($"\rRetry in {remaining:hh\\:mm\\:ss}");
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        }
        
        Console.WriteLine(); // Clear the countdown line
    }

    private static void ShowStaticWaitMessage(TimeSpan executionInterval)
    {
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var nextExecution = now.Add(executionInterval);
        var minuteText = executionInterval.TotalMinutes != 1 ? "minutes" : "minute";
        
        Console.WriteLine($"Current Time: {now:T}. " +
                         $"Next execution in {executionInterval.TotalMinutes:F0} {minuteText} at {nextExecution:T}.");
    }

    private void UpdateDnsRecords(List<PublicIPAdresse> currentIPs, WebRequest request,
        ResponseMessage<DNSRecords> info, IPType ipType)
    {
        var ipAddress = currentIPs.FirstOrDefault(x => x.Type == ipType);

        if (ipAddress is not { IP: not null } || string.IsNullOrEmpty(ipAddress.IP) || info.ResponseData == null)
            return;

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

    private static ResponseMessage<DNSRecords>? GetDnsRecords(WebRequest request,
        AccountInformation accountInformation,
        out List<PublicIPAdresse> currentIPs)
    {
        var info = request.GetDNSRecordInfo();
        currentIPs = request.GetCurrentPublicIP();

        if (info?.ResponseData == null)
            return null;

        info.ResponseData.RemoveHosts(accountInformation.IgnoredHosts.Hostnames);
        return info;
    }
}