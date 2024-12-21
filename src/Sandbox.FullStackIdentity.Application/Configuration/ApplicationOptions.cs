namespace Sandbox.FullStackIdentity.Application;

public sealed class ApplicationOptions
{

    public const string Key = "Application";

    public required ApplicationSecrets Secrets { get; set; }
    public required ApplicationDomains Domains { get; set; }
}


public sealed class ApplicationSecrets
{
    public string JwtSigningKey { get; set; } = string.Empty;
    public string SendgridApiKey { get; set; } = string.Empty;
    public string SendgridVerificationKey { get; set; } = string.Empty;
}

public sealed class ApplicationDomains
{
    public required string WebClient { get; set; }
    public required string PostgresDb { get; set; }
    public required string Redis { get; set; }
}


// Application__Secrets__JwtSigningKey
// Application__Secrets__SendgridApiKey
// Application__Secrets__SendgridVerificationKey
// Application__Domains__WebClient
// Application__Domains__PostgresDb
// Application__Domains__Redis
