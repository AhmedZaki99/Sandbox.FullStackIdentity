using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Internal;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{

    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="configureOptions">A delegate to configure <see cref="ApplicationOptions"/>.</param>
    /// <returns>An <see cref="AppBuilder"/> that can be used to further configure application services.</returns>
    public static AppBuilder AddAppServices(this IServiceCollection services, Action<ApplicationOptions>? configureOptions = null)
    {
        services.TryAddSingleton<ISystemClock, SystemClock>();

        services.AddScoped<ITenantValidator, TenantValidator>();

        services.AddScoped<IOrganizationAppService, OrganizationAppService>();
        services.AddScoped<IBookAppService, BookAppService>();
        
        services.AddScoped<IAccountEmailsAppService, AccountEmailsAppService>();
        services.AddScoped<ITokenCleanupAppService, TokenCleanupAppService>();

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }
        return new(services);
    }

    /// <summary>
    /// Adds app implementations for Identity user and role managers to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="configureOptions">A delegate to configure <see cref="IdentityOptions"/>.</param>
    /// <returns>The <see cref="IdentityBuilder"/> for further configuration of the identity system.</returns>
    public static IdentityBuilder AddAppManagers(this IdentityBuilder identityBuilder)
    {
        return identityBuilder
            .AddRoles<Role>()
            .AddUserManager<AppUserManager>();
    }

}
