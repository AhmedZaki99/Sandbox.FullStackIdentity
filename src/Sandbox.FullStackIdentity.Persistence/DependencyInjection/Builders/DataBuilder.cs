using Microsoft.Extensions.DependencyInjection;

namespace Sandbox.FullStackIdentity.DependencyInjection;

/// <summary>
/// A builder for configuring data-related services.
/// </summary>
public sealed class DataBuilder
{

    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> where data services are configured.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the connection string to the database.
    /// </summary>
    public required string ConnectionString { get; init; }


    internal DataBuilder(IServiceCollection services)
    {
        Services = services;
    }

}
