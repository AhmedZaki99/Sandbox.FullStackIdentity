using FluentResults;
using Flurl;
using HandlebarsDotNet;
using Microsoft.Extensions.Options;
using Sandbox.FullStackIdentity.Application.Internal;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <inheritdoc/>
internal sealed class AccountEmailsAppService : IAccountEmailsAppService
{

    #region Dependencies

    private readonly IEmailSender _emailSender;
    private readonly ITenantRepository _tenantRepository;
    private readonly IOptions<ApplicationOptions> _applicationOptions;

    public AccountEmailsAppService(
        IEmailSender emailSender,
        ITenantRepository tenantRepository,
        IOptions<ApplicationOptions> applicationOptions)
    {
        _emailSender = emailSender;
        _tenantRepository = tenantRepository;
        _applicationOptions = applicationOptions;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<Result> SendEmailConfirmationCodeAsync(User user, string code, CancellationToken cancellationToken = default)
    {
        var emailMessage = await RenderEmailTemplateAsync(
            ResourceLocator.Templates.EN.EmailConfirmationCode, 
            new
            {
                userName = user.UserName,
                code
            }, 
            cancellationToken);

        await _emailSender.SendAsync(user.Email, "Confirm your email", emailMessage, cancellationToken);
        return Result.Ok();
    }
    
    /// <inheritdoc/>
    public async Task<Result> SendInvitationLinkAsync(User user, string linkPath, string token, int tokenExpirationDays, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);
        if (tenant is null)
        {
            return new NotFoundError("Organization's tenant not found");
        }

        var linkUrl = Url
            .Combine(_applicationOptions.Value.Domains.WebClient, linkPath)
            .AppendQueryParam(new
            {
                userId = user.Id,
                token 
            });

        var emailMessage = await RenderEmailTemplateAsync(
            ResourceLocator.Templates.EN.AccountInvitationLink,
            new
            {
                organizationName = tenant.Handle,
                invitationLink = linkUrl,
                expirationDays = tokenExpirationDays
            },
            cancellationToken);

        await _emailSender.SendAsync(user.Email, "You have been invited to create an account", emailMessage, cancellationToken);
        return Result.Ok();
    }

    private static async Task<string> RenderEmailTemplateAsync(string templateResourceFile, object parameters, CancellationToken cancellationToken)
    {
        await using var emailTemplateStream = await ResourceLocator.ReadResourceFileAsync(templateResourceFile, cancellationToken);

        using var reader = new StreamReader(emailTemplateStream);

        var emailTemplateContent = await reader.ReadToEndAsync(cancellationToken);
        var emailTemplateRenderer = Handlebars.Compile(emailTemplateContent);

        return emailTemplateRenderer(parameters);
    }

    #endregion

}
