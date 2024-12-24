using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.FullStackIdentity.Presentation;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class PresentationServiceCollectionExtensions
{

    /// <summary>
    /// Adds api helper services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="jwtSigningKey">The JWT signing key.</param
    /// <param name="configureOptions">A delegate to configure <see cref="TokenAuthOptions"/>.</param>
    /// <returns>The <see cref="AppBuilder"/> to allow chaining up service configuration.</returns>
    public static AppBuilder AddApiServices(this AppBuilder builder, string jwtSigningKey, Action<TokenAuthOptions>? configureOptions = null)
    {
        builder.Services.AddSingleton(new TokenAuthSecrets(jwtSigningKey));

        builder.Services.AddScoped<IBearerTokenGenerator, JsonWebTokenGenerator>();

        if (configureOptions is not null)
        {
            builder.Services.Configure(configureOptions);
        }
        return builder;
    }

    /// <summary>
    /// Adds controllers and required filters to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="configureOptions">A delegate to configure <see cref="MvcOptions"/>.</param>
    /// <returns>An <see cref="IMvcBuilder"/> that can be used to further configure the MVC services.</returns>
    public static IMvcBuilder AddControllersAndFilters(this AppBuilder builder, Action<MvcOptions>? configureOptions = null)
    {
        return builder.Services.AddControllers(options =>
        {
            configureOptions?.Invoke(options);
            options.Filters.Add<MultiTenancyFilter>();
        });
    }

}
