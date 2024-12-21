namespace Sandbox.FullStackIdentity.Presentation;

public sealed class TokenAuthOptions
{
    public const string Key = "TokenAuth";

    public const int DefaultAccessTokenExpirationMinutes = 5;
    public const int DefaultRefreshTokenExpirationDays = 1;
    public const int DefaultRefreshTokenBytesLength = 16;

    public required string Issuer { get; set; }
    public required string Audience { get; set; }

    public int AccessTokenExpirationMinutes { get; set; } = DefaultAccessTokenExpirationMinutes;
    public int RefreshTokenExpirationDays { get; set; } = DefaultRefreshTokenExpirationDays;
    public int RefreshTokenBytesLength { get; set; } = DefaultRefreshTokenBytesLength;
}
