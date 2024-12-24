namespace Sandbox.FullStackIdentity.Presentation;

internal sealed class TokenAuthSecrets
{
    public string JwtSigningKey { get; }

    public TokenAuthSecrets(string jwtSigningKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtSigningKey);

        JwtSigningKey = jwtSigningKey;
    }
}
