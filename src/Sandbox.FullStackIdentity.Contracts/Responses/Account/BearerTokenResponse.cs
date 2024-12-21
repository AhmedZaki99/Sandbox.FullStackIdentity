namespace Sandbox.FullStackIdentity.Contracts;

public record BearerTokenResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpirationUtc);