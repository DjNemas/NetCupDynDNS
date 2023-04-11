using DynDNS.Models.Credential;
using System;
using System.IO;
using System.Text.Json;

namespace DynDNS
{
    public class CredentialFile
    {
        private FileInfo _File;

        public CredentialFile() 
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Credential.json");

            _File = new FileInfo(path); 
        }

        public CredentialFile CreateIfNotExist()
        {
            if (!_File.Exists)
            {
                UserCredential credential = new()
                {
                    ApiClientKey = "YourClientKeyHere",
                    ApiClientPW = "YourClientPWHere",
                    ApiCustomerNumber = 0,
                    Domain = "YourDomainHere"
                };

                byte[] jsonString = JsonSerializer.SerializeToUtf8Bytes(credential,
                    new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllBytes(_File.FullName, jsonString);
            }
            return this;
        }

        public UserCredential LoadCredential()
        {
            string jsonString = File.ReadAllText(_File.FullName);
            return JsonSerializer.Deserialize<UserCredential>(jsonString);
        }
    }
}
