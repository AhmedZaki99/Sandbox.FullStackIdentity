using Hope.Configuration.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Infrastructure;
using SendGrid.Extensions.DependencyInjection;

namespace Sandbox.FullStackIdentity.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{

    /// <summary>
    /// Adds email sender service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="sendGridApiKey">The SendGrid API key.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="EmailSenderOptions
    /// <returns>The <see cref="AppBuilder"/> to allow chaining up service configuration.</returns>
    public static AppBuilder AddEmailSender(this AppBuilder builder, string sendGridApiKey, Action<EmailSenderOptions>? configureOptions = null)
    {
        builder.Services.AddSendGrid((sp, options) =>
        {
            options.ApiKey = sendGridApiKey;
            options.ReliabilitySettings = sp.GetService<IOptions<EmailSenderOptions>>()?.Value.ReliabilitySettings ?? new();
        });

        builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
        builder.Services.AddTransient<IEmailCreator, EmptyEmailCreator>();


        if (configureOptions is not null)
        {
            builder.Services.Configure(configureOptions);
        }
        // Validation
        builder.Services.PostConfigure<EmailSenderOptions>(options => 
            options.RequireProperty(o => o.SenderEmailAddress));

        return builder;
    }

}
