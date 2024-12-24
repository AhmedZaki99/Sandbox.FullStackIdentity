using System.Net;
using Hangfire;
using Npgsql;
using Sandbox.FullStackIdentity.Background;
using Sandbox.FullStackIdentity.DependencyInjection;
using Sandbox.FullStackIdentity.Infrastructure;
using Sandbox.FullStackIdentity.Persistence;
using Serilog;

namespace Sandbox.FullStackIdentity.Services.Worker;

public class Program
{

    public static void Main(string[] args)
    {
        try
        {
            // Initialize
            var builder = WebApplication.CreateBuilder(args);

            // Configure
            builder.Configuration.AddKeyPerFile(builder.Configuration["SECRETS_DIRECTORY"] ?? "/run/secrets");
            ConfigureServices(builder);

            // Build
            var app = builder.Build();
            ConfigurePipeline(app);

            // Run
            app.Run();
        }
        catch (Exception ex) when (ex is not HostAbortedException and not OperationCanceledException)
        {
            using var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/StartupFailureLog.log")
                .CreateLogger();

            logger.Fatal(ex, "Application terminated unexpectedly.");
            throw;
        }
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Configuration binding.
        builder
            .ConfigureOptions<EmailSenderOptions>(EmailSenderOptions.Key, out var emailSenderOptions)
            .ConfigureOptions<LockingOptions>(LockingOptions.Key);

        emailSenderOptions.RequireProperty(o => o.SenderEmailAddress);

        var redisHost = builder.Configuration.GetRequiredValue("REDIS_HOST");
        var sendGridApiKey = builder.Configuration.GetRequiredValue("SENDGRID_API_KEY");

        var connectionString = GetDatabaseConnectionString(builder.Configuration);


        // Logging services.
        var serilogSqlOptions = new SerilogSqlOptions(connectionString);
        builder.Configuration.GetSection(SerilogSqlOptions.Key).Bind(serilogSqlOptions);

        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            loggerConfiguration.WriteToPostgreSQL(serilogSqlOptions);
        });


        // Background services.
        builder.Services
            .AddAppServices()
            .AddPostgresRepositories(connectionString)
            .AddEmailSender(sendGridApiKey)
            .AddRedLock([new DnsEndPoint(redisHost, 6379)])
            .AddBackgroundServices();

        builder.Services.AddHangfireServer();
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok());
    }


    private static string GetDatabaseConnectionString(ConfigurationManager configuration)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration["DATABASE_HOST"],
            Database = configuration["POSTGRES_USER"],
            Username = configuration["POSTGRES_USER"],
            Password = configuration["POSTGRES_PASSWORD"]
        };
        return builder.ConnectionString;
    }

}
