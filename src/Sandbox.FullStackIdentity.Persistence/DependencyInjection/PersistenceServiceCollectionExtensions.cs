using System.Data;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.FullStackIdentity.Domain;
using Sandbox.FullStackIdentity.Persistence;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{

    /// <summary>
    /// Adds repository services, including Hangfire client, with PostgreSQL implementation to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    /// Implementations are added for all repository services within the <see cref="Domain"/> namespace, in addition to:
    /// <list type="bullet">
    /// <item><see cref="IMultiTenancyContext"/> &amp; <see cref="IMultiTenancyInitializer"/></item>
    /// <item><see cref="IUserStore{TUser}"/> &amp; <see cref="IRoleStore{TUser}"/></item>
    /// <item><see cref="IBackgroundJobClient"/> &amp; <see cref="IRecurringJobManager"/></item>
    /// </list>
    /// </remarks>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <param name="hangfireSchemaName">The schema name for Hangfire tables.</param>
    /// <returns>The <see cref="AppBuilder"/> to allow chaining up service configuration.</returns>
    public static AppBuilder AddPostgresRepositories(this AppBuilder builder, string connectionString, string hangfireSchemaName = "hangfire")
    {
        // PostgreSQL database and Dapper.
        builder.Services.AddNpgsqlDataSource(connectionString, builder =>
        {
            // This enum is required since Npgsql doesn't have support for int to enum conversion within composite types.
            builder.MapEnum<PublicationStatus>();

            builder.MapComposite<BookDetails>();
        });

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        Dapper.SqlMapper.AddTypeMap(typeof(BookDetails), DbType.Object);


        // Repositories and Stores.
        builder.Services.AddScoped<MultiTenancyService>();
        builder.Services.AddScoped<IMultiTenancyContext>(sp => sp.GetRequiredService<MultiTenancyService>());
        builder.Services.AddScoped<IMultiTenancyInitializer>(sp => sp.GetRequiredService<MultiTenancyService>());

        builder.Services.AddScoped<IConfigRepository, ConfigRepository>();
        builder.Services.AddScoped<ITenantRepository, TenantRepository>();
        builder.Services.AddScoped<IBookRepository, BookRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        builder.Services.AddScoped<IUserStore<User>, UserStore>();
        builder.Services.AddScoped<IRoleStore<Role>, RoleStore>();


        // Hangfire
        builder.Services.AddSingleton<IConnectionFactory, HangfireConnectionFactory>();

        builder.Services.AddHangfire((sp, configuration) =>
            configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
                    options => options.UseConnectionFactory(sp.GetRequiredService<IConnectionFactory>()),
                    new PostgreSqlStorageOptions()
                    {
                        SchemaName = hangfireSchemaName
                    }));

        return builder;
    }

}
