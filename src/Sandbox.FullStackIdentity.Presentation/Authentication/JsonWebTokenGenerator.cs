using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation;

internal sealed class JsonWebTokenGenerator : IBearerTokenGenerator
{

    #region Dependencies

    private readonly UserManager<User> _userManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISystemClock _systemClock;
    private readonly TokenAuthSecrets _tokenAuthSecrets;
    private readonly IOptions<TokenAuthOptions> _tokenAuthOptions;
    private readonly ILogger<JsonWebTokenGenerator> _logger;

    public JsonWebTokenGenerator(
        UserManager<User> userManager,
        IRefreshTokenRepository refreshTokenRepository,
        ISystemClock systemClock,
        TokenAuthSecrets tokenAuthSecrets,
        IOptions<TokenAuthOptions> tokenAuthOptions,
        ILogger<JsonWebTokenGenerator> logger)
    {
        _userManager = userManager;
        _refreshTokenRepository = refreshTokenRepository;
        _systemClock = systemClock;
        _tokenAuthSecrets = tokenAuthSecrets;
        _tokenAuthOptions = tokenAuthOptions;
        _logger = logger;
    }

    #endregion

    #region Implementation

    public async Task<BearerTokenResponse?> GenerateAsync(User user, CancellationToken cancellationToken = default)
    {
        var refreshToken = await GenerateRefreshTokenAsync(user, cancellationToken: cancellationToken);
        if (refreshToken is null)
        {
            return null;
        }
        var accessToken = await GenerateAccessTokenAsync(user, cancellationToken);

        return new BearerTokenResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            RefreshTokenExpirationUtc: refreshToken.ExpiresOnUtc);
    }

    public async Task<BearerTokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetAsync(refreshToken, cancellationToken);
        if (existingToken?.User is null || existingToken.ExpiresOnUtc <= _systemClock.UtcNow.UtcDateTime)
        {
            return null;
        }

        var newRefreshToken = await GenerateRefreshTokenAsync(existingToken.User, existingToken, cancellationToken);
        if (newRefreshToken is null)
        {
            return null;
        }
        var accessToken = await GenerateAccessTokenAsync(existingToken.User, cancellationToken);

        return new BearerTokenResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken.Token,
            RefreshTokenExpirationUtc: newRefreshToken.ExpiresOnUtc);
    }


    public async Task RevokeAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetAsync(refreshToken, cancellationToken);
        if (existingToken?.User?.Id != userId)
        {
            return;
        }

        await _refreshTokenRepository.DeleteAsync(existingToken.Id, cancellationToken);
    }

    public Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _refreshTokenRepository.DeleteByUserAsync(userId, cancellationToken);
    }

    #endregion

    #region Helpers

    private async Task<string> GenerateAccessTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var expirationMinutes = Math.Max(_tokenAuthOptions.Value.AccessTokenExpirationMinutes, TokenAuthOptions.DefaultAccessTokenExpirationMinutes);
        var expirationTime = _systemClock.UtcNow.UtcDateTime.AddMinutes(expirationMinutes);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenAuthSecrets.JwtSigningKey));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = await BuildUserClaimsAsync(user, cancellationToken),
            Issuer = _tokenAuthOptions.Value.Issuer,
            Audience = _tokenAuthOptions.Value.Audience,
            Expires = expirationTime,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    private async Task<RefreshToken?> GenerateRefreshTokenAsync(User user, RefreshToken? existingToken = null, CancellationToken cancellationToken = default)
    {
        var tokenLength = Math.Max(_tokenAuthOptions.Value.RefreshTokenBytesLength, TokenAuthOptions.DefaultRefreshTokenBytesLength);
        var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(tokenLength));

        var expirationDays = Math.Max(_tokenAuthOptions.Value.RefreshTokenExpirationDays, TokenAuthOptions.DefaultRefreshTokenExpirationDays);
        var expirationTime = _systemClock.UtcNow.UtcDateTime.AddDays(expirationDays);

        var result = existingToken is not null
            ? await _refreshTokenRepository.UpdateAsync(existingToken.Id, tokenValue, expirationTime, cancellationToken)
            : await _refreshTokenRepository.CreateAsync(user.Id, tokenValue, expirationTime, cancellationToken);

        if (result.IsFailed)
        {
            _logger.LogWarning("Failed to generate refresh token for user {UserId}. Reason: {Error}", user.Id, result.Errors);
            return null;
        }
        return result.Value;
    }


    private async Task<ClaimsIdentity> BuildUserClaimsAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ApplicationClaimTypes.IsInvited, user.IsInvited.ToString()),
            new(ApplicationClaimTypes.GrantedPermission, user.GrantedPermission.ToString())
        };

        AddClaimIfNotNull(claims, JwtRegisteredClaimNames.GivenName, user.FirstName);
        AddClaimIfNotNull(claims, JwtRegisteredClaimNames.FamilyName, user.LastName);
        AddClaimIfNotNull(claims, ApplicationClaimTypes.TenantId, user.TenantId?.ToString());
        
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new ClaimsIdentity(claims);
    }

    private static void AddClaimIfNotNull(List<Claim> claims, string type, string? value)
    {
        if (value is not null)
        {
            claims.Add(new(type, value));
        }
    }

    #endregion

}
