using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Internal;
using Sandbox.FullStackIdentity.Application;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{

    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <returns>An <see cref="AppBuilder"/> that can be used to further configure application services.</returns>
    public static AppBuilder AddAppServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ISystemClock, SystemClock>();

        services.AddScoped<ITenantValidator, TenantValidator>();

        services.AddScoped<IOrganizationAppService, OrganizationAppService>();
        services.AddScoped<IUserAppService, UserAppService>();
        services.AddScoped<IBookAppService, BookAppService>();
        
        services.AddScoped<IAccountEmailsAppService, AccountEmailsAppService>();
        services.AddScoped<ITokenCleanupAppService, TokenCleanupAppService>();

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
            .AddRoles<IdentityRole<Guid>>()
            .AddUserManager<AppUserManager>();
    }

}
