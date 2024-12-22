using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.DependencyInjection;
using Sandbox.FullStackIdentity.Domain;
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
            .ConfigureOptions<ApplicationOptions>(ApplicationOptions.Key, out var applicationOptions)
            .ConfigureOptions<TokenAuthOptions>(TokenAuthOptions.Key, out var tokenAuthOptions);

        var connectionString = GetDatabaseConnectionString(builder.Configuration, applicationOptions);


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
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenAuthOptions.Issuer,
                    ValidAudience = tokenAuthOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(applicationOptions.Secrets.JwtSigningKey))
                };
            });


        // Data protection.
        var dataProtectionOptions = builder.Configuration.GetSection(DataProtectionOptions.Key).Get<DataProtectionOptions>()
            ?? throw new InvalidOperationException($"The required '{DataProtectionOptions.Key}' is not found in configuration.");

        var cert = X509CertificateLoader.LoadPkcs12FromFile(dataProtectionOptions.CertPath, null);
        builder.Services
            .AddDataProtection()
            .SetApplicationName(dataProtectionOptions.AppName)
            .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(applicationOptions.Domains.Redis), dataProtectionOptions.StoreKey)
            .ProtectKeysWithCertificate(cert)
            .UnprotectKeysWithAnyCertificate(cert);


        // Identity services.
        builder.Services
            .AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                options.ClaimsIdentity.UserNameClaimType = JwtRegisteredClaimNames.Name;
                options.ClaimsIdentity.EmailClaimType = JwtRegisteredClaimNames.Email;
            })
            .AddDefaultTokenProviders()
            .AddAppManagers();


        // Application services.
        builder.Services
            .AddAppServices()
            .AddPostgresRepositories(connectionString)
            .AddEmailSender()
            .AddApiServices()
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


    private static string GetDatabaseConnectionString(ConfigurationManager configuration, ApplicationOptions applicationOptions)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = applicationOptions.Domains.PostgresDb,
            Database = configuration["POSTGRES_USER"],
            Username = configuration["POSTGRES_USER"],
            Password = configuration["POSTGRES_PASSWORD"]
        };
        return builder.ConnectionString;
    }

}
