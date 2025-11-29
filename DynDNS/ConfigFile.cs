using System;
using System.IO;
using System.Text.Json;
using DynDNS.Models.AccountInformation;

namespace DynDNS;

/// <summary>
/// Manages the configuration file for storing account information.
/// </summary>
internal class ConfigFile
{
    private readonly FileInfo _file;
    public const string FileName = "dyndns-updater-config.json";

    public ConfigFile()
    {
        var path = Path.Combine(Environment.CurrentDirectory, FileName);

        _file = new FileInfo(path);
    }

    public bool DoesConfigFileExist()
    {
        return _file.Exists;
    }

    public AccountInformation? LoadAccountInformation()
    {
        var jsonString = File.ReadAllText(_file.FullName);
        return JsonSerializer.Deserialize<AccountInformation>(jsonString);
    }

    public void StoreAccountInformation(AccountInformation accountInformation)
    {
        var jsonString = JsonSerializer.SerializeToUtf8Bytes(accountInformation,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllBytes(_file.FullName, jsonString);
    }

    public void DeleteFile()
    {
        if (DoesConfigFileExist()) File.Delete(_file.FullName);
    }
}