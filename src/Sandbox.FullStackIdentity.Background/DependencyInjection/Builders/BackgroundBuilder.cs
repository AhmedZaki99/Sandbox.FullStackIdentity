using Microsoft.Extensions.DependencyInjection;

namespace Sandbox.FullStackIdentity.DependencyInjection;

/// <summary>
/// A builder for configuring background services.
/// </summary>
public sealed class BackgroundBuilder
{

    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> where background services are configured.
    /// </summary>
    public IServiceCollection Services { get; }


    public BackgroundBuilder(IServiceCollection services)
    {
        Services = services;
    }

}
