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

internal class UpdaterService
{
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromMinutes(
        int.Parse(Environment.GetEnvironmentVariable(EnvironmentVariables.ExecutionIntervalInMinutes) ?? "60"));

    private const int ShutdownTimeInSeconds = 30;

    public void RunDnsUpdate(AccountInformation accountInformation, int? executionIntervalInMinutes, bool tickingClock)
    {
        var request = new WebRequest(accountInformation.Credentials);
        var shouldBeKeptAlive = executionIntervalInMinutes is > 0;

        do
        {
            if (request.LoginClient())
            {
                var info = GetDnsRecords(request, accountInformation, out var currentIPs);

                UpdateDnsRecords(currentIPs, request, info, IPType.IPv4);

                UpdateDnsRecords(currentIPs, request, info, IPType.IPv6);

                request.LogoutClient();

                if (shouldBeKeptAlive)
                {
                    StartWaitTimer(tickingClock);
                    Console.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Closing application in {ShutdownTimeInSeconds} seconds.");
                Task.Delay(TimeSpan.FromSeconds(ShutdownTimeInSeconds)).Wait();
                shouldBeKeptAlive = false;
            }
        } while (shouldBeKeptAlive);
    }

    private static void StartWaitTimer(bool tickingClock)
    {
        if (tickingClock)
        {
            var waitTime = ExecutionInterval;
            while (waitTime.Ticks > 0)
            {
                Console.Write($"\rRetry in {waitTime}");
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                waitTime = waitTime.Subtract(TimeSpan.FromSeconds(1));
            }
        }
        else
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var minuteText = ExecutionInterval.TotalMinutes > 1 ? "minutes" : "minute";
            var logMessage =
                $"Current Time: {now.ToLongTimeString()}. " +
                $"Next execution in {ExecutionInterval.TotalMinutes} {minuteText} at " +
                $"{now.AddMinutes(ExecutionInterval.TotalMinutes).ToLongTimeString()}.";
            Console.WriteLine(logMessage);
            Task.Delay(ExecutionInterval).Wait();
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