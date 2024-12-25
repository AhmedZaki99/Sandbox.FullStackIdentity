using Hope.Configuration;

namespace Sandbox.FullStackIdentity.Presentation;

public sealed class TokenAuthOptions : IKeyedOptions
{
    public const string Key = "TokenAuth";
    string IKeyedOptions.Key => Key;


    public const int DefaultAccessTokenExpirationMinutes = 5;
    public const int DefaultRefreshTokenExpirationDays = 1;
    public const int DefaultRefreshTokenBytesLength = 16;

    public string? Issuer { get; set; } 
    public string? Audience { get; set; } 

    public int AccessTokenExpirationMinutes { get; set; } = DefaultAccessTokenExpirationMinutes;
    public int RefreshTokenExpirationDays { get; set; } = DefaultRefreshTokenExpirationDays;
    public int RefreshTokenBytesLength { get; set; } = DefaultRefreshTokenBytesLength;
}
