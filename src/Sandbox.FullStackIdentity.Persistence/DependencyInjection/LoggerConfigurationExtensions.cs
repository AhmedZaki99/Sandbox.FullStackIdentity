using Sandbox.FullStackIdentity.Persistence;
using Serilog;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Adds a sink which writes to the PostgreSQL table.
    /// </summary>
    /// <param name="options">PostgreSQL logging options.</param>
    /// <returns>The <see cref="LoggerConfiguration"/> to allow chaining up configuration.</returns>
    public static LoggerConfiguration WriteToPostgreSQL(this LoggerConfiguration loggerConfiguration, SerilogSqlOptions options)
    {
        loggerConfiguration.WriteTo.PostgreSQL(
            connectionString: options.ConnectionString,
            tableName: options.TableName,
            schemaName: options.SchemaName,
            needAutoCreateTable: options.NeedAutoCreateTable,
            needAutoCreateSchema: options.NeedAutoCreateSchema,
            loggerColumnOptions: options.LoggerColumnOptions,
            loggerPropertyColumnOptions: options.LoggerPropertyColumnOptions,
            restrictedToMinimumLevel: options.RestrictedToMinimumLevel);

        return loggerConfiguration;
    }
}
