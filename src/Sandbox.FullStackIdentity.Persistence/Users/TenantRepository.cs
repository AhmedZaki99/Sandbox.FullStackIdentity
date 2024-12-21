using System.Data.Common;
using Dapper;
using FluentResults;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class TenantRepository : ITenantRepository
{

    #region Dependencies

    private readonly DbDataSource _dbDataSource;
    private readonly IMultiTenancyContext _multiTenancyContext;

    public TenantRepository(DbDataSource dbDataSource, IMultiTenancyContext multiTenancyContext)
    {
        _dbDataSource = dbDataSource;
        _multiTenancyContext = multiTenancyContext;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<Tenant?> GetAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<Tenant>(
            """
            SELECT * FROM tenants WHERE id = @tenantId LIMIT 1
            """,
            new { tenantId });
    }
    
    /// <inheritdoc/>
    public async Task<Tenant?> FindByHandleAsync(string handle, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<Tenant>(
            """
            SELECT * FROM tenants WHERE handle = @handle LIMIT 1
            """,
            new { handle });
    }

    /// <inheritdoc/>
    public async Task<List<User>> ListUsersAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<User>(
            """
            SELECT * FROM users WHERE tenant_id = @tenantId
            """,
            new { tenantId });

        return result.ToList();
    }


    /// <inheritdoc/>
    public async Task<Result<Tenant>> CreateAsync(string handle, string? name = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Handle = handle,
            Name = name
        };

        await connection.ExecuteAsync(
            $"""
            INSERT INTO tenants (id, handle, name) 
            VALUES (
                @{nameof(Tenant.Id)}, 
                @{nameof(Tenant.Handle)}, 
                @{nameof(Tenant.Name)})
            """,
            tenant);

        return tenant;
    }

    /// <inheritdoc/>
    public async Task<Result> ChangeNameAsync(string name, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        int count = await connection.ExecuteAsync(
            """
            UPDATE tenants SET name = @name 
            WHERE is_deleted = FALSE AND id = @tenantId
            """,
            new { tenantId, name });

        if (count < 1)
        {
            return new NotFoundError("Tenant is not found.");
        }
        return Result.Ok();
    }


    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        int count = await connection.ExecuteAsync(
            """
            UPDATE tenants SET is_deleted = TRUE 
            WHERE is_deleted = FALSE AND id = @tenantId
            """,
            new { tenantId });

        if (count < 1)
        {
            return new NotFoundError("Tenant is not found.");
        }
        return Result.Ok();
    }
    
    /// <inheritdoc/>
    public async Task<Result> DeletePermanentlyAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        int count = await connection.ExecuteAsync(
            """
            DELETE FROM tenants WHERE id = @tenantId
            """,
            new { tenantId });

        if (count < 1)
        {
            return new NotFoundError("Tenant is not found.");
        }
        return Result.Ok();
    }

    #endregion

}
