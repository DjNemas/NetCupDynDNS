using System;
using System.IO;
using System.Text.Json;
using DynDNS.Models.AccountInformation;

namespace DynDNS;

internal class AccountInformationFile
{
    private readonly FileInfo _file;
    public const string FileName = "AccountInformation.json";

    public AccountInformationFile()
    {
        var path = Path.Combine(Environment.CurrentDirectory, FileName);

        _file = new FileInfo(path);
    }

    public AccountInformationFile CreateIfNotExist()
    {
        if (_file.Exists) return this;

        AccountInformation accountInformation = new();

        var jsonString = JsonSerializer.SerializeToUtf8Bytes(accountInformation,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllBytes(_file.FullName, jsonString);
        return this;
    }

    public AccountInformation LoadAccountInformation()
    {
        var jsonString = File.ReadAllText(_file.FullName);
        return JsonSerializer.Deserialize<AccountInformation>(jsonString);
    }
}