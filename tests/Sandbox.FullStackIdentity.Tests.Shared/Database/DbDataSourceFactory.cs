using System.Data;
using System.Data.Common;
using Npgsql;
using Respawn;
using Sandbox.FullStackIdentity.Domain;
using Testcontainers.PostgreSql;
using Xunit;

namespace Sandbox.FullStackIdentity.Tests.Shared;

public sealed class DbDataSourceFactory : IAsyncLifetime
{
    
    private PostgreSqlContainer? _dbContainer;
    private Respawner? _respawner;


    public async Task InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder().Build();
        await _dbContainer.StartAsync();

        _respawner = await Respawner.CreateAsync(_dbContainer.GetConnectionString(), new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task DisposeAsync()
    {
        if (_dbContainer is not null)
        {
            await _dbContainer.DisposeAsync();
        }
    }


    public DbDataSource CreateDbContext()
    {
        if (_dbContainer is null)
        {
            throw new InvalidOperationException("Factory data is not initialized");
        }
        var connectionString = _dbContainer.GetConnectionString();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapComposite<BookDetails>();

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        Dapper.SqlMapper.AddTypeMap(typeof(BookDetails), DbType.Object);

        return dataSourceBuilder.Build();
    }

    public async Task ResetDatabaseAsync()
    {
        if (_dbContainer is null || _respawner is null)
        {
            throw new InvalidOperationException("Factory data is not initialized");
        }

        await _respawner.ResetAsync(_dbContainer.GetConnectionString());
    }

}
