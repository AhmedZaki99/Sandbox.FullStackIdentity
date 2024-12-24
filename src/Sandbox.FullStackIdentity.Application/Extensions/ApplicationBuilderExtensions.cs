using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class ApplicationBuilderExtensions
{

    public static IHostApplicationBuilder ConfigureOptions<T>(this IHostApplicationBuilder builder, string optionsKey)
        where T : class
    {
        var configSection = builder.Configuration.GetSection(optionsKey);
        builder.Services.Configure<T>(configSection);

        return builder;
    }
    
    public static IHostApplicationBuilder ConfigureOptions<T>(this IHostApplicationBuilder builder, string optionsKey, out T options, T? defaultValue = null)
        where T : class
    {
        var configSection = builder.Configuration.GetSection(optionsKey);
        options = configSection.Get<T>()
            ?? defaultValue
            ?? throw new InvalidOperationException($"{optionsKey} configuration is missing.");

        builder.Services.Configure<T>(configSection);
        return builder;
    }

}
