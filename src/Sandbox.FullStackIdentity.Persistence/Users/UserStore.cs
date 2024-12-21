using System.Data.Common;
using Dapper;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class UserStore : IUserStore<User>, ISoftUserStore<User>, IUserEmailStore<User>, IUserPasswordStore<User>, IUserSecurityStampStore<User>, IUserRoleStore<User>, IUserLockoutStore<User>
{

    #region Dependencies

    private readonly DbDataSource _dbDataSource;

    public UserStore(DbDataSource dbDataSource)
    {
        _dbDataSource = dbDataSource;
    }

    #endregion


    #region IUserStore Implementation

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(user.UserName);
    }

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }


    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        user.Id = Guid.NewGuid();

        var insertCount = await connection.ExecuteAsync(
            $"""
            INSERT INTO users (
                id, 
                tenant_id, 
                is_invited, 
                invitation_accepted, 
                granted_permission, 
                user_name, 
                normalized_user_name, 
                email, 
                normalized_email, 
                email_confirmed, 
                password_hash, 
                security_stamp, 
                lockout_end, 
                lockout_enabled, 
                access_failed_count, 
                first_name, 
                last_name)
            VALUES (
                @{nameof(User.Id)}, 
                @{nameof(User.TenantId)}, 
                @{nameof(User.IsInvited)}, 
                @{nameof(User.InvitationAccepted)}, 
                @{nameof(User.GrantedPermission)}, 
                @{nameof(User.UserName)}, 
                @{nameof(User.NormalizedUserName)}, 
                @{nameof(User.Email)}, 
                @{nameof(User.NormalizedEmail)}, 
                @{nameof(User.EmailConfirmed)}, 
                @{nameof(User.PasswordHash)}, 
                @{nameof(User.SecurityStamp)}, 
                @{nameof(User.LockoutEnd)}, 
                @{nameof(User.LockoutEnabled)}, 
                @{nameof(User.AccessFailedCount)}, 
                @{nameof(User.FirstName)}, 
                @{nameof(User.LastName)})
            """,
            user);

        return insertCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not insert user {user.Email}." });
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var updateCount = await connection.ExecuteAsync(
            $"""
            UPDATE users 
            SET (
                granted_permission, 
                invitation_accepted, 
                user_name, 
                normalized_user_name, 
                email, 
                normalized_email, 
                email_confirmed, 
                password_hash, 
                security_stamp,
                lockout_end, 
                lockout_enabled, 
                access_failed_count,
                first_name, 
                last_name) 
            = (
                @{nameof(User.GrantedPermission)}, 
                @{nameof(User.InvitationAccepted)}, 
                @{nameof(User.UserName)}, 
                @{nameof(User.NormalizedUserName)}, 
                @{nameof(User.Email)}, 
                @{nameof(User.NormalizedEmail)}, 
                @{nameof(User.EmailConfirmed)}, 
                @{nameof(User.PasswordHash)}, 
                @{nameof(User.SecurityStamp)},             
                @{nameof(User.LockoutEnd)}, 
                @{nameof(User.LockoutEnabled)}, 
                @{nameof(User.AccessFailedCount)}, 
                @{nameof(User.FirstName)}, 
                @{nameof(User.LastName)})
            WHERE is_deleted = FALSE AND id = @{nameof(User.Id)}
            """,
            user);

        return updateCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not update user {user.Email}." });
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            UPDATE users SET is_deleted = TRUE 
            WHERE is_deleted = FALSE AND id = @userId
            """,
            new { userId = user.Id });

        return IdentityResult.Success;
    }


    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<User>(
            $"""
            SELECT * FROM users WHERE is_deleted = FALSE AND id = @userId LIMIT 1
            """,
            new { userId });
    }

    public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<User>(
            $"""
            SELECT * FROM users WHERE is_deleted = FALSE AND normalized_user_name = @normalizedUserName LIMIT 1
            """,
            new { normalizedUserName });
    }

    #endregion

    #region ISoftUserStore Implementation

    public async Task<IdentityResult> DeletePermanentlyAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            """
            DELETE FROM users WHERE id = @userId
            """,
            new { userId = user.Id });

        return IdentityResult.Success;
    }

    #endregion

    #region IUserEmailStore Implementation

    public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(user.Email);
    }

    public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }


    public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<User>(
            $"""
            SELECT * FROM users WHERE is_deleted = FALSE AND normalized_email = @normalizedEmail LIMIT 1
            """,
            new { normalizedEmail });
    }

    #endregion

    #region IUserPasswordStore Implementation

    public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PasswordHash is not null);
    }

    #endregion

    #region IUserSecurityStampStore Implementation

    public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.SecurityStamp);
    }

    #endregion

    #region IUserRoleStore Implementation

    public async Task AddToRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var roleId = await GetRoleIdAsync(connection, normalizedRoleName)
            ?? throw new InvalidOperationException($"Failed to add user to role. Role not found: {normalizedRoleName}");

        await connection.ExecuteAsync(
            """
            INSERT INTO user_roles (user_id, role_id) VALUES (@userId, @roleId)
            """,
            new { userId = user.Id, roleId });
    }

    public async Task RemoveFromRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var roleId = await GetRoleIdAsync(connection, normalizedRoleName)
            ?? throw new InvalidOperationException($"Failed to add user to role. Role not found: {normalizedRoleName}");

        await connection.ExecuteAsync(
            """
            DELETE FROM user_roles WHERE user_id = @userId AND role_id = @roleId
            """,
            new { userId = user.Id, roleId });
    }

    public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<string>(
            """
            SELECT r.name FROM roles r
            INNER JOIN user_roles ur ON r.id = ur.role_id
            WHERE ur.user_id = @userId
            """,
            new { userId = user.Id });

        return result.ToList();
    }

    public async Task<bool> IsInRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1 FROM roles r
                INNER JOIN user_roles ur ON r.id = ur.role_id
                WHERE ur.user_id = @userId AND r.normalized_name = @normalizedRoleName
            )
            """,
            new { userId = user.Id, normalizedRoleName });
    }

    public async Task<IList<User>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<User>(
            """
            SELECT u.* FROM users u
            INNER JOIN user_roles ur ON u.id = ur.user_id
            INNER JOIN roles r ON r.id = ur.role_id
            WHERE r.normalized_name = @normalizedRoleName
            """,
            new { normalizedRoleName });

        return result.ToList();
    }

    #endregion

    #region IUserLockoutStore Implementation

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.LockoutEnabled);
    }

    public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    #endregion

    #region IDisposable Implementation

    void IDisposable.Dispose() { }

    #endregion


    #region Helpers

    private static Task<Guid?> GetRoleIdAsync(DbConnection connection, string normalizedRoleName)
    {
        return connection.ExecuteScalarAsync<Guid?>(
            """
            SELECT id FROM roles WHERE normalized_name = @normalizedRoleName LIMIT 1
            """,
            new { normalizedRoleName });
    }

    #endregion

}
