using System.Security.Cryptography.X509Certificates;
using System.Text;
using Hope.Configuration.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Sandbox.FullStackIdentity.DependencyInjection;
using Sandbox.FullStackIdentity.Domain;
using Sandbox.FullStackIdentity.Infrastructure;
using Sandbox.FullStackIdentity.Persistence;
using Sandbox.FullStackIdentity.Presentation;
using Serilog;
using StackExchange.Redis;

namespace Sandbox.FullStackIdentity.Services.HostApi;

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
            .ConfigureOptions<TokenAuthOptions>(TokenAuthOptions.Key, out var tokenAuthOptions);

        emailSenderOptions.RequireProperty(o => o.SenderEmailAddress);

        var redisHost = builder.Configuration.GetRequiredValue("REDIS_HOST");
        var jwtSigningKey = builder.Configuration.GetRequiredValue("JWT_SIGNING_KEY");
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


        // Authentication services.
        builder.Services.AddAuthorization();
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = tokenAuthOptions.Issuer,
                    ValidAudience = tokenAuthOptions.Audience,
                    ValidateIssuer = tokenAuthOptions.Issuer is not null,
                    ValidateAudience = tokenAuthOptions.Audience is not null,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey))
                };
            });


        // Data protection.
        var dataProtectionOptions = builder.Configuration
            .GetRequiredObject<DataProtectionOptions>(DataProtectionOptions.Key)
            .RequireProperty(o => o.CertPath);

        var cert = X509CertificateLoader.LoadPkcs12FromFile(dataProtectionOptions.CertPath, null);
        builder.Services
            .AddDataProtection()
            .SetApplicationName(dataProtectionOptions.AppName)
            .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisHost), dataProtectionOptions.StoreKey)
            .ProtectKeysWithCertificate(cert)
            .UnprotectKeysWithAnyCertificate(cert);


        // Identity services.
        builder.Services
            .AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddDefaultTokenProviders()
            .AddAppManagers();


        // Application services.
        builder.Services
            .AddAppServices()
            .AddPostgresRepositories(connectionString)
            .AddEmailSender(sendGridApiKey)
            .AddApiServices(jwtSigningKey)
            .AddControllersAndFilters();

        builder.Services.AddOpenApi();
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseMiddleware<TaskCanceledMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
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
