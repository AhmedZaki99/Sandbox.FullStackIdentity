using System.Data.Common;
using Dapper;
using Sandbox.FullStackIdentity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class RoleStore : IRoleStore<Role>
{

    #region Dependencies

    private readonly DbDataSource _dbDataSource;

    public RoleStore(DbDataSource dbDataSource)
    {
        _dbDataSource = dbDataSource;
    }

    #endregion

    #region IRoleStore Implementation

    public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string?> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(role.Name);
    }

    public Task SetRoleNameAsync(Role role, string? roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(Role role, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }


    public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        role.Id = Guid.NewGuid();

        var insertCount = await connection.ExecuteAsync(
            $"""
            INSERT INTO roles (id, name, normalized_name) VALUES (
                @{nameof(Role.Id)}, 
                @{nameof(Role.Name)}, 
                @{nameof(Role.NormalizedName)})
            """,
            role);

        return insertCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not insert role {role.Name}." });
    }

    public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var updateCount = await connection.ExecuteAsync(
            $"""
            UPDATE roles SET (name, normalized_name) = (
                @{nameof(Role.Name)}, 
                @{nameof(Role.NormalizedName)}) 
            WHERE id = @{nameof(Role.Id)}
            """,
            role);

        return updateCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not update role {role.Name}." });
    }

    public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var deleteCount = await connection.ExecuteAsync(
            $"""
            DELETE FROM roles WHERE id = @{nameof(Role.Id)}
            """,
            role);

        return deleteCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not delete role {role.Name}." });
    }


    public async Task<Role?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<Role>(
            """
            SELECT * FROM roles WHERE id = @roleId LIMIT 1
            """,
            new { roleId });
    }

    public async Task<Role?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<Role>(
            """
            SELECT * FROM roles WHERE normalized_name = @normalizedRoleName LIMIT 1
            """,
            new { normalizedRoleName });
    }

    #endregion

    #region IDisposable Implementation

    void IDisposable.Dispose() { }

    #endregion

}
