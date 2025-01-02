using System.Data.Common;
using System.Text.Json;
using Dapper;
using FluentResults;
using Hope.Identity.Dapper;
using Hope.Results;
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
    public async Task<PagedList<User>> ListUsersAsync(Guid? tenantId = null, PaginationParams? paginationParams = null, CancellationToken cancellationToken = default)
    {
        paginationParams ??= new();
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        using var multi = await connection.QueryMultipleAsync(
            $"""
            SELECT COUNT(*) FROM identity.users
            WHERE tenant_id = @tenantId;

            SELECT * FROM identity.users
            WHERE tenant_id = @tenantId;
            ORDER BY user_name
            LIMIT @PageSize OFFSET @Offset;
            """,
            new
            {
                tenantId,
                paginationParams.PageSize,
                paginationParams.Offset
            });

        var count = await multi.ReadFirstAsync<int>();
        var users = await multi.ReadAsync<User>();

        return new PagedList<User>
        {
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = count,
            Items = users.ToList()
        };
    }


    /// <inheritdoc/>
    public async Task<Result<Tenant>> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        if (tenant.Id == Guid.Empty)
        {
            tenant.Id = Guid.NewGuid();
        }

        string[] propertyNames = [
            nameof(Tenant.Id),
            nameof(Tenant.Handle),
            nameof(Tenant.Name),
            nameof(Tenant.BlacklistedEmails)
        ];

        await connection.ExecuteAsync(
            $"""
            INSERT INTO tenants {propertyNames.BuildSqlColumnsBlock(JsonNamingPolicy.SnakeCaseLower)} 
            VALUES {propertyNames.BuildSqlParametersBlock()}
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
    public async Task<Result> ChangeHandleAsync(string handle, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        int count = await connection.ExecuteAsync(
            """
            UPDATE tenants SET handle = @handle 
            WHERE is_deleted = FALSE AND id = @tenantId
            """,
            new { tenantId, handle });

        if (count < 1)
        {
            return new NotFoundError("Tenant is not found.");
        }
        return Result.Ok();
    }


    /// <inheritdoc/>
    public async Task<Result<string[]>> AddToBlacklistAsync(string[] emails, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var blacklistedEmails = await connection.ExecuteScalarAsync<string[]>(
            """
            UPDATE tenants 
            SET blacklisted_emails = (
                SELECT array_agg(DISTINCT email) 
                FROM unnest(array_cat(blacklisted_emails, @emails)) AS email
            )
            WHERE is_deleted = FALSE AND id = @tenantId
            RETURNING blacklisted_emails
            """,
            new { tenantId, emails });

        if (blacklistedEmails is null)
        {
            return new NotFoundError("Tenant is not found.");
        }
        return blacklistedEmails;
    }

    /// <inheritdoc/>
    public async Task<Result<string[]>> RemoveFromBlacklistAsync(string[] emails, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var blacklistedEmails = await connection.ExecuteScalarAsync<string[]>(
            """
            UPDATE tenants 
            SET blacklisted_emails = (
                SELECT array_agg(email) 
                FROM (
                    SELECT unnest(blacklisted_emails) AS email
                    EXCEPT
                    SELECT unnest(@emails)
                )
            )
            WHERE is_deleted = FALSE AND id = @tenantId
            RETURNING blacklisted_emails
            """,
            new { tenantId, emails });

        if (blacklistedEmails is null)
        {
            return new NotFoundError("Tenant is not found.");
        }
        return blacklistedEmails;
    }

    /// <inheritdoc/>
    public async Task<Result<string[]>> UpdateBlacklistAsync(string[] blacklistedEmails, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        tenantId ??= _multiTenancyContext.CurrentTenantId;
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        int count = await connection.ExecuteAsync(
            """
            UPDATE tenants SET blacklisted_emails = @blacklistedEmails
            WHERE is_deleted = FALSE AND id = @tenantId
            """,
            new { tenantId, blacklistedEmails });

        if (count < 1)
        {
            return new NotFoundError("Tenant is not found.");
        }
        return blacklistedEmails;
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
