using Microsoft.Extensions.DependencyInjection;

namespace Sandbox.FullStackIdentity.DependencyInjection;

/// <summary>
/// A builder for configuring application services.
/// </summary>
public sealed class AppBuilder
{

    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> where application services are configured.
    /// </summary>
    public IServiceCollection Services { get; }


    internal AppBuilder(IServiceCollection services)
    {
        Services = services;
    }

}
