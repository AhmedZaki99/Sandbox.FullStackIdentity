using FluentResults;

namespace Sandbox.FullStackIdentity.Domain;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetAsync(string token, CancellationToken cancellationToken = default);

    Task<Result<RefreshToken>> CreateAsync(Guid userId, string token, DateTime expiresOnUtc, CancellationToken cancellationToken = default);
    Task<Result<RefreshToken>> UpdateAsync(Guid tokenId, string token, DateTime expiresOnUtc, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid tokenId, CancellationToken cancellationToken = default);
    Task<int> DeleteByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
