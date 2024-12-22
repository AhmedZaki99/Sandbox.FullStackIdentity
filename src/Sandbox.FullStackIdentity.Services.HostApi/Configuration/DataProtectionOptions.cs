namespace Sandbox.FullStackIdentity.Services.HostApi;

public sealed class DataProtectionOptions
{
    public const string Key = "DataProtection";

    public string AppName { get; set; } = nameof(FullStackIdentity);
    public string StoreKey { get; set; } = Key;
    public required string CertPath { get; set; }
}
