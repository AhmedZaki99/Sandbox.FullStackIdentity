using Hangfire.PostgreSql;
using Npgsql;

namespace Sandbox.FullStackIdentity.Persistence;

internal sealed class HangfireConnectionFactory : IConnectionFactory
{
    private readonly NpgsqlDataSource _dbDataSource;

    public HangfireConnectionFactory(NpgsqlDataSource dbDataSource)
    {
        _dbDataSource = dbDataSource;
    }

    public NpgsqlConnection GetOrCreateConnection()
    {
        return _dbDataSource.CreateConnection();
    }
}
