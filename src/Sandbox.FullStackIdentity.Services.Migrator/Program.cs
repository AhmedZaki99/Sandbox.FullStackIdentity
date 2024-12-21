using DbUp;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Reflection;

namespace Sandbox.FullStackIdentity.Services.Migrator;

internal class Program
{

    public static int Main(string[] args)
    {
        var secretsPath = Environment.GetEnvironmentVariable("SECRETS_DIRECTORY") ?? "/run/secrets";
        var configuration = new ConfigurationBuilder()
            .AddCommandLine(args)
            .AddEnvironmentVariables()
            .AddKeyPerFile(secretsPath)
            .Build();

        var connectionString = GetDatabaseConnectionString(configuration);
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransaction()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            Console.WriteLine(result.Error);
            return -1;
        }
        return 0;
    }


    private static string GetDatabaseConnectionString(IConfiguration configuration)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration["ClientEndpoints:Database:HostUrl"],
            Username = configuration["POSTGRES_USER"],
            Password = configuration["POSTGRES_PASSWORD"]
        };
        return builder.ConnectionString;
    }

}
