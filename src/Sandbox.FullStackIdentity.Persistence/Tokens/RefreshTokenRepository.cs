using System.Data.Common;
using Dapper;
using FluentResults;
using Hope.Results;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{

    #region Dependencies

    private readonly DbDataSource _dbDataSource;

    public RefreshTokenRepository(DbDataSource dbDataSource)
    {
        _dbDataSource = dbDataSource;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<RefreshToken?> GetAsync(string token, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<RefreshToken, User, RefreshToken>(
            """
            SELECT * FROM refresh_tokens rt
            INNER JOIN users u ON rt.user_id = u.id
            WHERE rt.token = @token 
            LIMIT 1
            """,
            (refreshToken, user) =>
            {
                refreshToken.User = user;
                return refreshToken;
            },
            param: new { token },
            splitOn: "id");

        return result.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<Result<RefreshToken>> CreateAsync(Guid userId, string token, DateTime expiresOnUtc, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresOnUtc = expiresOnUtc.ToUniversalTime()
        };

        await connection.ExecuteAsync(
            $"""
            INSERT INTO refresh_tokens (id, user_id, token, expires_on_utc)
            VALUES (
                @{nameof(RefreshToken.Id)}, 
                @{nameof(RefreshToken.UserId)}, 
                @{nameof(RefreshToken.Token)}, 
                @{nameof(RefreshToken.ExpiresOnUtc)})
            """,
            refreshToken);

        return refreshToken;
    }

    /// <inheritdoc/>
    public async Task<Result<RefreshToken>> UpdateAsync(Guid tokenId, string token, DateTime expiresOnUtc, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<RefreshToken>(
            """
            UPDATE refresh_tokens
            SET (token, expires_on_utc) = (@token, @expiresOnUtc)
            WHERE id = @tokenId
            RETURNING *
            """,
            new { tokenId, token, expiresOnUtc });

        if (result is null)
        {
            return new NotFoundError("Token is not found.");
        }
        return result;
    }


    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        int count = await connection.ExecuteAsync(
            """
            DELETE FROM refresh_tokens WHERE id = @tokenId
            """,
            new { tokenId });

        if (count < 1)
        {
            return new NotFoundError("Token is not found.");
        }
        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<int> DeleteByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteAsync(
            """
            DELETE FROM refresh_tokens WHERE user_id = @userId
            """,
            new { userId });
    }

    /// <inheritdoc/>
    public async Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteAsync(
            """
            DELETE FROM refresh_tokens WHERE expires_on_utc < (NOW() AT TIME ZONE 'UTC')
            """);
    }

    #endregion

}
