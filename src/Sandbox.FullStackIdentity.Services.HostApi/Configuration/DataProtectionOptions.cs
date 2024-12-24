using Sandbox.FullStackIdentity.Application;

namespace Sandbox.FullStackIdentity.Services.HostApi;

public sealed class DataProtectionOptions : IKeyedOptions
{
    public const string Key = "DataProtection";
    string IKeyedOptions.Key => Key;


    public string AppName { get; set; } = nameof(FullStackIdentity);
    public string StoreKey { get; set; } = Key;
    public string CertPath { get; set; } = string.Empty;
}
