using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation;

public interface IBearerTokenGenerator
{
    Task<BearerTokenResponse?> GenerateAsync(User user, CancellationToken cancellationToken = default);
    Task<BearerTokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task RevokeAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
}
