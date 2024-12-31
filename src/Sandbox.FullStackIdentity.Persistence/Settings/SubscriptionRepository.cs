using System.Data.Common;
using System.Text;
using Dapper;
using FluentResults;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class SubscriptionRepository : ISubscriptionRepository
{

    #region Dependencies

    private readonly DbDataSource _dbDataSource;

    public SubscriptionRepository(DbDataSource dbDataSource)
    {
        _dbDataSource = dbDataSource;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<bool?> IsSubscribedAsync(string email, SubscriptionScope scope, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool?>(
            """
            SELECT is_subscribed FROM subscription_preferences 
            WHERE email = @email AND scope IN (@scope, -1)
            LIMIT 1
            """,
            new { email, scope });
    }
    
    /// <inheritdoc/>
    public async Task<string[]> GetSubscribedEmailsAsync(SubscriptionScope scope, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<string>(
            """
            SELECT email FROM subscription_preferences 
            WHERE is_subscribed = TRUE AND scope IN (@scope, -1)
            """,
            new { scope });

        return result.ToArray();
    }
    
    /// <inheritdoc/>
    public async Task<string[]> GetUnsubscribedEmailsAsync(SubscriptionScope scope, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<string>(
            """
            SELECT email FROM subscription_preferences 
            WHERE is_subscribed = FALSE AND scope IN (@scope, -1)
            """,
            new { scope });

        return result.ToArray();
    }


    /// <inheritdoc/>
    public Task<Result> SubscribeAsync(string email, SubscriptionScope scope, CancellationToken cancellationToken = default) => 
        SubscribeAsync(email, [scope], cancellationToken);

    /// <inheritdoc/>
    public async Task<Result> SubscribeAsync(string email, SubscriptionScope[] scopes, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { email });
        var sqInsertParams = BuildInsertQueryParameters(dynamicParams, scopes, subscribe: true);

        // 1. Skip if the global "is_subscribed = TRUE" preference exists already.
        // 2. Delete the global "is_subscribed = FALSE" preference if it exists.
        await connection.ExecuteAsync(
            $"""
            DO $$ 
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM subscription_preferences WHERE (email, scope, is_subscribed) = (@email, -1, TRUE)
                ) THEN
                    DELETE FROM subscription_preferences WHERE (email, scope, is_subscribed) = (@email, -1, FALSE);

                    INSERT INTO subscription_preferences (email, scope, is_subscribed)
                    VALUES {sqInsertParams}
                    ON CONFLICT (email, scope) DO UPDATE SET is_subscribed = EXCLUDED.is_subscribed;
                END IF;
            END $$;
            """,
            dynamicParams);

        return Result.Ok();
    }


    /// <inheritdoc/>
    public Task<Result> UnsubscribeAsync(string email, SubscriptionScope scope, CancellationToken cancellationToken = default) =>
        UnsubscribeAsync(email, [scope], cancellationToken);

    /// <inheritdoc/>
    public async Task<Result> UnsubscribeAsync(string email, SubscriptionScope[] scopes, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { email });
        var sqInsertParams = BuildInsertQueryParameters(dynamicParams, scopes, subscribe: false);

        // 1. Skip if the global "is_subscribed = FALSE" preference exists already.
        // 2. Delete the global "is_subscribed = TRUE" preference if it exists.
        await connection.ExecuteAsync(
            $"""
            DO $$ 
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM subscription_preferences WHERE (email, scope, is_subscribed) = (@email, -1, FALSE)
                ) THEN
                    DELETE FROM subscription_preferences WHERE (email, scope, is_subscribed) = (@email, -1, TRUE);

                    INSERT INTO subscription_preferences (email, scope, is_subscribed)
                    VALUES {sqInsertParams}
                    ON CONFLICT (email, scope) DO UPDATE SET is_subscribed = EXCLUDED.is_subscribed;
                END IF;
            END $$;
            """,
            dynamicParams);

        return Result.Ok();
    }


    /// <inheritdoc/>
    public async Task<Result> UpdateSubscriptionsAsync(string email, Dictionary<SubscriptionScope, bool> scopeDictionary, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { email });
        var sqInsertParams = BuildInsertQueryParameters(dynamicParams, scopeDictionary);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM subscription_preferences WHERE email = @email AND scope = -1;

            INSERT INTO subscription_preferences (email, scope, is_subscribed) 
            VALUES {sqInsertParams}
            ON CONFLICT (email, scope) DO UPDATE SET is_subscribed = EXCLUDED.is_subscribed
            """,
            dynamicParams);

        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<Result> ResetSubscriptionsAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM subscription_preferences WHERE email = @email;
            """,
            email);

        return Result.Ok();
    }
    

    /// <inheritdoc/>
    public async Task<Result> SubscribeToAllAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM subscription_preferences WHERE email = @email;

            INSERT INTO subscription_preferences (email, scope, is_subscribed)
            VALUES (@email, -1, TRUE);
            """,
            email);

        return Result.Ok();
    }
    
    /// <inheritdoc/>
    public async Task<Result> UnsubscribeToAllAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM subscription_preferences WHERE email = @email;

            INSERT INTO subscription_preferences (email, scope, is_subscribed)
            VALUES (@email, -1, FALSE);
            """,
            email);

        return Result.Ok();
    }

    #endregion

    #region Helpers

    private static string BuildInsertQueryParameters(DynamicParameters dynamicParams, SubscriptionScope[] scopes, bool subscribe)
    {
        return BuildInsertQueryParameters(dynamicParams, scopes.ToDictionary(scope => scope, _ => subscribe));
    }

    private static string BuildInsertQueryParameters(DynamicParameters dynamicParams, Dictionary<SubscriptionScope, bool> scopeDictionary)
    {
        int counter = 0;
        var sqlParamsBuilder = new StringBuilder();

        foreach (var (scope, isSubscribed) in scopeDictionary)
        {
            if (counter > 0)
            {
                sqlParamsBuilder.Append(',');
            }
            var scopeParam = $"scope{counter}";
            var isSubscribedParam = $"isSubscribed{counter}";

            sqlParamsBuilder.Append($"(@email, @{scopeParam}, @{isSubscribedParam})");
            dynamicParams.Add(scopeParam, scope);
            dynamicParams.Add(isSubscribedParam, isSubscribed);

            counter++;
        }
        return sqlParamsBuilder.ToString();
    }

    #endregion

}
