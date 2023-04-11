using DynDNS.Models.Actions;
using DynDNS.Models.Credential;
using DynDNS.Models.DNSInfoResponse;
using DynDNS.Models.ResponseMessageDataType;
using System;
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

        public bool DNSRecordDestionationChanged(ResponseMessage<DNSRecords> records)
        {
            if (records.ResponseData.DnsRecords[0].Destination == GetCurrentPublicIP())
                return false;
            else
                return true;
        }

        public void UpdateDNSRecord(DNSRecords recordsSet)
        {
            string newIP = GetCurrentPublicIP();
            IPAddress temp;
            foreach (var record in recordsSet.DnsRecords)
            {
                if (IPAddress.TryParse(record.Destination, out temp))
                {
                    record.Destination = newIP;
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

        private string GetCurrentPublicIP()
        {
            using (HttpClient client = new HttpClient())
            {
                return client.GetStringAsync("https://api.ipify.org/").Result;
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
