using Microsoft.Extensions.Configuration;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        return configuration[key]
            ?? throw new InvalidOperationException($"The required '{key}' variable is not found in configuration.");
    }
    
    public static T GetRequiredObject<T>(this IConfiguration configuration, string key)
    {
        return configuration.GetSection(key).Get<T>()
            ?? throw new InvalidOperationException($"The required '{key}' variable is not found in configuration.");
    }
}
