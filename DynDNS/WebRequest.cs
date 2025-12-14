using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DynDNS.Models.AccountInformation;
using DynDNS.Models.Actions;
using DynDNS.Models.DNSInfoResponse;
using DynDNS.Models.PublicIP;
using DynDNS.Models.ResponseMessageDataType;

namespace DynDNS;

internal class WebRequest
{
    private static class IdentifyOwnIpAddressEndpoints
    {
        public const string Ipv4 = "https://api.ipify.org";
        public const string Ipv6 = "https://api6.ipify.org";
    }
    
    private readonly HttpClient _Client;
    private readonly uint _ApiCustomerNumber;
    private readonly string _DomainURL;
    private readonly string _ApiClientPW;
    private readonly string _ApiClientKey;
    private readonly Uri _EndPointURL = new Uri("https://ccp.netcup.net/run/webservice/servers/endpoint.php?JSON");
    private readonly JsonSerializerOptions _jsonOptions;

    private string? _ApiSessionID;

    public WebRequest(UserCredential credentials)
    {
        _ApiCustomerNumber = credentials.ApiCustomerNumber;
        _DomainURL = credentials.Domain;
        _ApiClientPW = credentials.ApiClientPW;
        _ApiClientKey = credentials.ApiClientKey;

        _Client = new HttpClient();
        _Client.BaseAddress = _EndPointURL;

        _jsonOptions = new JsonSerializerOptions
        {
            Converters = { 
                 new ResponseMessageConverter<SessionID>(),
                 new ResponseMessageConverter<DNSRecords>(),
                 new ResponseMessageConverter<string>() 
            }
        };
    }

    public bool DNSRecordDestionationChanged(IPType type, ResponseMessage<DNSRecords> records, List<PublicIPAdresse> currentIPs)
    {
        bool hasDifferentDestination = false;

        PublicIPAdresse? ipAdresse = currentIPs.FirstOrDefault(x => x.Type == type);

        if (ipAdresse == null || string.IsNullOrEmpty(ipAdresse.IP) || records.ResponseData == null)
            return false;

        foreach (var record in records.ResponseData.DnsRecords)
        {
            if (!IPAddress.TryParse(record.Destination, out IPAddress? ip))
                continue;

            switch (type)
            {
                case IPType.IPv4:
                    if (ip.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    break;
                case IPType.IPv6:
                    if (ip.AddressFamily != AddressFamily.InterNetworkV6)
                        continue;
                    break;
            }
                
            if (record.Destination != ipAdresse.IP)
                hasDifferentDestination = true;
        }
        return hasDifferentDestination;
    }

    public void UpdateDNSRecord(IPType type, DNSRecords recordsSet, List<PublicIPAdresse> currentIPs)
    {
        PublicIPAdresse? ipAdresse = currentIPs.FirstOrDefault(x => x.Type == type);
        if(ipAdresse == null)
            return;

        foreach (var record in recordsSet.DnsRecords)
        {
            if (!IPAddress.TryParse(record.Destination, out IPAddress? ip))
                continue;

            switch (type)
            {
                case IPType.IPv4:
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        record.Destination = ipAdresse.IP;
                    break;
                case IPType.IPv6:
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                        record.Destination = ipAdresse.IP;
                    break;
            }
        }

        RequestAction<UpdateDnsRecords> action = new()
        {
            Action = "updateDnsRecords",
            Param = new UpdateDnsRecords()
            {
                DomainName = _DomainURL,
                CustonmerNumber = _ApiCustomerNumber,
                ApiKey = _ApiClientKey,
                ApiSessionID = _ApiSessionID ?? string.Empty,
                DNSRecordSet = recordsSet
            }
        };

        string jsonString = SendActionPost(action).Content.ReadAsStringAsync().Result;

        ResponseMessage<DNSRecords>? updateResponse;
        try
        {
            updateResponse = JsonSerializer.Deserialize<ResponseMessage<DNSRecords>>(jsonString, _jsonOptions);
        }
        catch (JsonException ex)
        {
            PrintErrorRed($"Failed to deserialize DNS update response: {ex.Message}");
            PrintErrorRed($"JSON Response: {jsonString}");
            return;
        }

        if (updateResponse == null || updateResponse.IsError)
        {
            if (updateResponse != null)
            {
                PrintErrorRed("DNS Update failed:");
                PrintErrorRed($"Status: {updateResponse.Status}");
                PrintErrorRed($"Status Code: {updateResponse.StatusCode}");
                PrintErrorRed($"Short Message: {updateResponse.ShortMessage}");
                if (!string.IsNullOrEmpty(updateResponse.LongMessage))
                    PrintErrorRed($"Long Message: {updateResponse.LongMessage}");
            }
            return;
        }
        Console.WriteLine("Domain DNS Destionation Updated!");
    }

    public List<PublicIPAdresse> GetCurrentPublicIP()
    {
        using var client = new HttpClient();
        var list = new List<PublicIPAdresse>();

        PublicIPAdresse ipv4 = new()
        {
            Type = IPType.IPv4
        };
        PublicIPAdresse ipv6 = new()
        {
            Type = IPType.IPv6
        };

        try
        {
            Console.WriteLine("Try getting public IPv4 IP");
            ipv4.IP = client.GetStringAsync(IdentifyOwnIpAddressEndpoints.Ipv4).Result;
            Console.WriteLine("Got IPv4 public IP");
        }
        catch (Exception ex)
        {
            Console.WriteLine("IPv4 not Supported on your current connection.");
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
#endif
        }
        try
        {
            Console.WriteLine("Try getting public IPv6 IP");
            ipv6.IP = client.GetStringAsync(IdentifyOwnIpAddressEndpoints.Ipv6).Result;
            Console.WriteLine("Got IPv6 public IP");
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
#endif
            Console.WriteLine("IPv6 not Supported on your current connection.");
        }

        list.Add(ipv4);
        list.Add(ipv6);

        return list;
    }

    public ResponseMessage<DNSRecords>? GetDNSRecordInfo()
    {
        RequestAction<InfoDnsRecords> acton = new()
        {
            Action = "infoDnsRecords",
            Param = new InfoDnsRecords()
            {
                ApiKey = _ApiClientKey,
                ApiSessionID = _ApiSessionID ?? string.Empty,
                CustonmerNumber = _ApiCustomerNumber,
                DomainName = _DomainURL
            }
        };

        HttpResponseMessage response = SendActionPost(acton);
        string jsonString = response.Content.ReadAsStringAsync().Result;

        ResponseMessage<DNSRecords>? dnsResponse;
        try
        {
            dnsResponse = JsonSerializer.Deserialize<ResponseMessage<DNSRecords>>(jsonString, _jsonOptions);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to deserialize DNS record info response: {ex.Message}");
            return null;
        }

        if (dnsResponse == null || dnsResponse.IsError)
        {
            if (dnsResponse != null)
            {
                Console.WriteLine("Recive DNS Zone Data failed:");
                Console.WriteLine($"Status: {dnsResponse.Status}");
                Console.WriteLine($"Status Code: {dnsResponse.StatusCode}");
                Console.WriteLine($"Short Message: {dnsResponse.ShortMessage}");
                if (!string.IsNullOrEmpty(dnsResponse.LongMessage))
                    Console.WriteLine($"Long Message: {dnsResponse.LongMessage}");
            }
            return null;
        }            

        Console.WriteLine("Recive DNS Zone Data success!");
        return dnsResponse;
    }

    public bool LoginClient()
    {
        RequestAction<ClientLogin> action = new()
        {
            Action = "login",
            Param = new ClientLogin()
            {
                ApiKey = _ApiClientKey,
                ApiPassword = _ApiClientPW,
                CustomerNumber = _ApiCustomerNumber
            }
        };

        string jsonString = SendActionPost(action).Content.ReadAsStringAsync().Result;

        ResponseMessage<SessionID>? response;
        try
        {
            response = JsonSerializer.Deserialize<ResponseMessage<SessionID>>(jsonString, _jsonOptions);
        }
        catch (JsonException ex)
        {
            PrintErrorRed($"Failed to deserialize login response: {ex.Message}");
            return false;
        }

        if (response == null || response.IsError)
        {
            if (response != null)
            {
                PrintErrorRed($"Login failed! Please Check your Credentials in {ConfigFile.FileName} File.");
                PrintErrorRed($"Status: {response.Status}");
                PrintErrorRed($"Status Code: {response.StatusCode}");
                PrintErrorRed($"Short Message: {response.ShortMessage}");
                if (!string.IsNullOrEmpty(response.LongMessage))
                    PrintErrorRed($"Long Message: {response.LongMessage}");
            }
            return false;
        }

        if (response.ResponseData != null)
            _ApiSessionID = response.ResponseData.ApiSessionID;
        Console.WriteLine("Login success!");
        return true;
    }

    public void LogoutClient()
    {
        RequestAction<ClientLogout> action = new()
        {
            Action = "logout",
            Param = new ClientLogout()
            {
                ApiKey = _ApiClientKey,
                CustomerNumber = _ApiCustomerNumber,
                ApiSessionID = _ApiSessionID ?? string.Empty
            }
        };

        HttpResponseMessage response = SendActionPost(action);
        string jsonString = response.Content.ReadAsStringAsync().Result;

        ResponseMessage<string>? logoutResponse;
        try
        {
            logoutResponse = JsonSerializer.Deserialize<ResponseMessage<string>>(jsonString, _jsonOptions);
        }
        catch (JsonException ex)
        {
            PrintErrorRed($"Failed to deserialize logout response: {ex.Message}");
            return;
        }

        if (logoutResponse == null || logoutResponse.IsError)
        {
            if (logoutResponse != null)
            {
                PrintErrorRed("Logout failed:");
                PrintErrorRed($"Status: {logoutResponse.Status}");
                PrintErrorRed($"Status Code: {logoutResponse.StatusCode}");
                PrintErrorRed($"Short Message: {logoutResponse.ShortMessage}");
                if (!string.IsNullOrEmpty(logoutResponse.LongMessage))
                    PrintErrorRed($"Long Message: {logoutResponse.LongMessage}");
            }
            return;
        }
        Console.WriteLine("Logout success!");
    }

    private HttpResponseMessage SendActionPost(object action)
    {
        string json = JsonSerializer.Serialize(action);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage? response = null;
            
        bool success = false;
        TimeSpan waitTime = TimeSpan.FromMinutes(1);
        do
        {                
            try
            {
                response = _Client.PostAsync("", content).Result;
                success = true;
            }
            catch (AggregateException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                while (waitTime.Ticks > 0)
                {
                    waitTime = waitTime.Subtract(TimeSpan.FromSeconds(1));
                    Console.Write($"\rNo Internet Connection... Retry in {waitTime}");
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                }
                waitTime = TimeSpan.FromMinutes(1);
                Console.ResetColor();
                Console.WriteLine();
            }
        } while (!success);
        return response!;
    }

    private void PrintErrorRed(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();
    }
}