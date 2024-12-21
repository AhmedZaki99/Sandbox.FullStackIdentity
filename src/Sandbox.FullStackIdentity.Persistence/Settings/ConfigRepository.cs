using System.Data.Common;
using Dapper;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class ConfigRepository : IConfigRepository
{

    #region Dependencies

    private readonly DbDataSource _dbDataSource;

    public ConfigRepository(DbDataSource dbDataSource)
    {
        _dbDataSource = dbDataSource;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<string?>(
            """
            SELECT value FROM global_configs WHERE key = @key LIMIT 1
            """,
            new { key });
    }
    
    /// <inheritdoc/>
    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            """
            INSERT INTO global_configs (key, value)
            VALUES (@key, @value)
            ON CONFLICT (key) 
            DO UPDATE SET value = EXCLUDED.value;
            """,
            new { key, value });
    }

    #endregion

}
