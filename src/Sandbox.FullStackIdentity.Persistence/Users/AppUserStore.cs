﻿using System.Data.Common;
using Dapper;
using Hope.Identity.Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Persistence;

public sealed class AppUserStore : DapperUserStore<User, IdentityRole<Guid>, Guid>, IUserInvitationStore<User>, ISoftUserStore<User>
{
    private readonly string _tablePrefix;
    private readonly string _isDeletedColumn;

    public AppUserStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer = null) 
        : base(dbDataSource, options, describer) 
    {
        _tablePrefix = Options.TableSchema is null ? string.Empty : $"{Options.TableSchema}.";
        _isDeletedColumn = nameof(User.IsDeleted).ToSqlColumn(Options.TableNamingPolicy);
    }


    protected override string GetBaseUserSqlCondition(DynamicParameters sqlParameters, string tableAlias = "")
    {
        var columnPrefix = string.IsNullOrEmpty(tableAlias) ? string.Empty : $"{tableAlias}.";

        return $"{columnPrefix}{_isDeletedColumn} = FALSE";
    }

    public override async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { userId = user.Id });

        await connection.ExecuteAsync(
            $"""
            UPDATE {_tablePrefix}{Options.UserNames.Table} SET {_isDeletedColumn} = TRUE 
            WHERE {GetBaseUserSqlCondition(dynamicParams)} 
            AND {Options.UserNames.Id} = @userId
            """,
            dynamicParams);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeletePermanentlyAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {_tablePrefix}{Options.UserNames.Table} WHERE {Options.UserNames.Id} = @userId
            """,
            new { userId = user.Id });

        return IdentityResult.Success;
    }


    public Task<bool> GetIsInvitedAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        return Task.FromResult(user.IsInvited);
    }

    public Task SetIsInvitedAsync(User user, bool isInvited, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        user.IsInvited = isInvited;
        return Task.CompletedTask;
    }

    public Task<bool> GetInvitationAcceptedAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        return Task.FromResult(user.InvitationAccepted);
    }

    public Task SetInvitationAcceptedAsync(User user, bool invitationAccepted, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        user.InvitationAccepted = invitationAccepted;
        return Task.CompletedTask;
    }
}
