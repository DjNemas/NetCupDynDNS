using DynDNS.Models.Actions;
using DynDNS.Models.Credential;
using DynDNS.Models.DNSInfoResponse;
using DynDNS.Models.PublicIP;
using DynDNS.Models.ResponseMessageDataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace DynDNS
{
    internal class WebRequest
    {
        private readonly HttpClient _Client;
        private readonly uint _ApiCustomerNumber;
        private readonly string _DomainURL;
        private readonly string _ApiClientPW;
        private readonly string _ApiClientKey;
        private readonly Uri _EndPointURL = new Uri("https://ccp.netcup.net/run/webservice/servers/endpoint.php?JSON");

        private string _ApiSessionID;

        public WebRequest(UserCredential credentials)
        {
            _ApiCustomerNumber = credentials.ApiCustomerNumber;
            _DomainURL = credentials.Domain;
            _ApiClientPW = credentials.ApiClientPW;
            _ApiClientKey = credentials.ApiClientKey;

            _Client = new HttpClient();
            _Client.BaseAddress = _EndPointURL;
        }

        public bool DNSRecordDestionationChanged(IPType type, ResponseMessage<DNSRecords> records, List<PublicIPAdresse> currentIPs)
        {
            bool hasDifferentDestination = false;

            PublicIPAdresse ipAdresse = currentIPs.FirstOrDefault(x => x.Type == type);

            if (ipAdresse != null && string.IsNullOrEmpty(ipAdresse.IP))
                hasDifferentDestination = false;

            foreach (var record in records.ResponseData.DnsRecords)
            {
                IPAddress ip = null;
                IPAddress.TryParse(record.Destination, out ip);
                if (ip == null)
                    continue;

                switch (type)
                {
                    case IPType.IPv4:
                        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                            continue;
                        break;
                    case IPType.IPv6:
                        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                            continue;
                        break;
                }
                
                if (ipAdresse != null && record.Destination != ipAdresse.IP)
                    hasDifferentDestination = true;
            }
            return hasDifferentDestination;
        }

        public void UpdateDNSRecord(IPType type, DNSRecords recordsSet, List<PublicIPAdresse> currentIPs)
        {
            PublicIPAdresse ipAdresse = currentIPs.FirstOrDefault(x => x.Type == type);
            if(ipAdresse == null)
                return;

            foreach (var record in recordsSet.DnsRecords)
            {
                IPAddress ip = null;
                IPAddress.TryParse(record.Destination, out ip);

                if (ip == null)
                    continue;

                switch (type)
                {
                    case IPType.IPv4:
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            record.Destination = ipAdresse.IP;
                        break;
                    case IPType.IPv6:
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                            record.Destination = ipAdresse.IP;
                        break;
                }
            }

            RequestAction<UpdateDnsRecords> action = new()
            {
                action = "updateDnsRecords",
                param = new UpdateDnsRecords()
                {
                    DomainName = _DomainURL,
                    CustonmerNumber = _ApiCustomerNumber,
                    ApiKey = _ApiClientKey,
                    ApiSessionID = _ApiSessionID,
                    DNSRecordSet = recordsSet
                }
            };

            string jsonString = SendActionPost(action).Content.ReadAsStringAsync().Result;

            // Deserilize respone of correct type
            ResponseMessage<DNSRecords> statusOkResponse = null;
            ResponseMessage<string> statusErrorRespone = null;
            try
            {
                statusOkResponse = JsonSerializer.Deserialize<ResponseMessage<DNSRecords>>(jsonString);
            }
            catch
            {
                statusErrorRespone = JsonSerializer.Deserialize<ResponseMessage<string>>(jsonString);
            }

            // If Update not work print response message
            if (statusErrorRespone != null && statusErrorRespone.Status != "success")
            {
                PrintErrorRed("DNS Update failed:");
                PrintErrorResponse(statusErrorRespone);
                return;
            }
            Console.WriteLine("Domain DNS Destionation Updated!");
        }

        public List<PublicIPAdresse> GetCurrentPublicIP()
        {
            using (HttpClient client = new HttpClient())
            {
                List<PublicIPAdresse> list = new List<PublicIPAdresse>();

                PublicIPAdresse ipv4 = new();
                ipv4.Type = IPType.IPv4;
                PublicIPAdresse ipv6 = new();
                ipv6.Type = IPType.IPv6;
                
                try
                {
                    Console.WriteLine("Try getting public IPv4 IP");
                    ipv4.IP = client.GetStringAsync("https://api4.my-ip.io/ip").Result;
                    Console.WriteLine("Got IPv4 public IP");
                }
                catch 
                {
                    Console.WriteLine("IPv4 not Supported on your current connection.");
                }
                try
                {
                    Console.WriteLine("Try getting public IPv6 IP");
                    ipv6.IP = client.GetStringAsync("https://api6.my-ip.io/ip").Result;
                    Console.WriteLine("Got IPv6 public IP");
                }
                catch 
                {
                    Console.WriteLine("IPv6 not Supported on your current connection.");
                }

                list.Add(ipv4);
                list.Add(ipv6);

                return list;
            }
        }

        public ResponseMessage<DNSRecords> GetDNSRecordInfo()
        {
            RequestAction<InfoDnsRecords> acton = new()
            {
                action = "infoDnsRecords",
                param = new InfoDnsRecords()
                {
                    ApiKey = _ApiClientKey,
                    ApiSessionID = _ApiSessionID,
                    CustonmerNumber = _ApiCustomerNumber,
                    DomainName = _DomainURL
                }
            };

            HttpResponseMessage response = SendActionPost(acton);
            string jsonString = response.Content.ReadAsStringAsync().Result;

            // Deserilize respone of correct type
            ResponseMessage<DNSRecords> statusOkResponse = null;
            ResponseMessage<string> statusErrorRespone = null;
            try
            {
                statusOkResponse = JsonSerializer.Deserialize<ResponseMessage<DNSRecords>>(jsonString);
            }
            catch
            {
                statusErrorRespone = JsonSerializer.Deserialize<ResponseMessage<string>>(jsonString);
            }

            // If Login not work print response message
            if (statusErrorRespone != null && statusErrorRespone.Status != "success")
            {
                Console.WriteLine("Recive DNS Zone Data failed:");
                PrintErrorResponse(statusErrorRespone);
                return null;
            }            

            Console.WriteLine("Recive DNS Zone Data success!");
            return statusOkResponse;
        }

        public bool LoginClient()
        {
            // Create action methode content
            RequestAction<ClientLogin> action = new()
            {
                action = "login",
                param = new ClientLogin()
                {
                    ApiKey = _ApiClientKey,
                    ApiPassword = _ApiClientPW,
                    CustomerNumber = _ApiCustomerNumber
                }
            };

            // Send Post request
            string jsonString = SendActionPost(action).Content.ReadAsStringAsync().Result;

            // Deserilize respone of correct type
            ResponseMessage<SessionID> loginOkResponse = null;
            ResponseMessage<string> loginErrorRespone = null;
            try
            {
                loginOkResponse = JsonSerializer.Deserialize<ResponseMessage<SessionID>>(jsonString);
            }
            catch
            {
                loginErrorRespone = JsonSerializer.Deserialize<ResponseMessage<string>>(jsonString);
            }

            // If Login not work print response message
            if (loginErrorRespone != null && loginErrorRespone.Status != "success")
            {
                PrintErrorRed("Login failed! Please Check your Credentials in Credential.json File.");
                PrintErrorResponse(loginErrorRespone);
                return false;
            }

            // store sessionID
            if (loginOkResponse != null)
                _ApiSessionID = loginOkResponse.ResponseData.ApiSessionID;
            Console.WriteLine("Login success!");
            return true;
        }

        public void LogoutClient()
        {
            RequestAction<ClientLogout> action = new()
            {
                action = "logout",
                param = new ClientLogout()
                {
                    ApiKey = _ApiClientKey,
                    CustomerNumber = _ApiCustomerNumber,
                    ApiSessionID = _ApiSessionID
                }
            };

            // Send Post request
            HttpResponseMessage response = SendActionPost(action);
            string jsonString = SendActionPost(action).Content.ReadAsStringAsync().Result;

            // Deserilize respone
            var logoutResponse = JsonSerializer.Deserialize<ResponseMessage<string>>(jsonString);

            // If Login not work print response message
            if(logoutResponse != null && logoutResponse.Status != "success")
            {
                PrintErrorRed("Logout failed:");
                PrintErrorResponse(logoutResponse);
                return;
            }
            Console.WriteLine("Logout success!");
        }

        private HttpResponseMessage SendActionPost(object action)
        {
            string json = JsonSerializer.Serialize(action);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            return _Client.PostAsync("", content).Result;
        }

        private void PrintErrorResponse(ResponseMessage<string> response)
        {
            if (response != null)
            {
                foreach (PropertyInfo propertyInfo in response.GetType().GetProperties())
                {
                    PrintErrorRed(propertyInfo.Name + ": " + propertyInfo.GetValue(response));
                }
                return;
            }
        }

        private void PrintErrorRed(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

    }
}
