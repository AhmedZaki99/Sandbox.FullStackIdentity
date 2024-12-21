using Microsoft.Extensions.DependencyInjection;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Infrastructure;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{

    /// <summary>
    /// Adds email sender service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <returns>The <see cref="AppBuilder"/> to allow chaining up service configuration.</returns>
    public static AppBuilder AddEmailSender(this AppBuilder builder)
    {
        builder.Services.AddTransient<IEmailSender, LoggerEmailSender>();
        builder.Services.AddKeyedTransient<IEmailSender, SendGridEmailSender>("empty");

        return builder;
    }

}
