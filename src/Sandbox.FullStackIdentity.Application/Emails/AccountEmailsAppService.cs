using FluentResults;
using Hope.Results;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <inheritdoc/>
internal sealed class AccountEmailsAppService : IAccountEmailsAppService
{

    #region Dependencies

    private readonly IEmailSender _emailSender;
    private readonly ITenantRepository _tenantRepository;

    public AccountEmailsAppService(
        IEmailSender emailSender,
        ITenantRepository tenantRepository)
    {
        _emailSender = emailSender;
        _tenantRepository = tenantRepository;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<Result> SendEmailConfirmationCodeAsync(User user, string code, CancellationToken cancellationToken = default)
    {
        var emailData = new
        {
            name = user.UserName,
            code
        };

        await _emailSender.SendAsync(user.Email, EmailTemplateKeys.ConfirmEmail, emailData, cancellationToken);
        return Result.Ok();
    }
    

    /// <inheritdoc/>
    public async Task<Result> SendResetPasswordLinkAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        var emailData = new
        {
            name = user.UserName,
            userId = user.Id,
            token
        };

        await _emailSender.SendAsync(user.Email, EmailTemplateKeys.ResetPassword, emailData, cancellationToken);
        return Result.Ok();
    }
    
    /// <inheritdoc/>
    public async Task<Result> SendChangeEmailLinkAsync(User user, string token, string email, CancellationToken cancellationToken = default)
    {
        var emailData = new
        {
            name = user.UserName,
            userId = user.Id,
            email,
            token
        };

        await _emailSender.SendAsync(email, EmailTemplateKeys.ConfirmEmailChange, emailData, cancellationToken);
        return Result.Ok();
    }
    

    /// <inheritdoc/>
    public async Task<Result> SendInvitationLinkAsync(User user, string token, int tokenExpirationDays, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);
        if (tenant is null)
        {
            return new NotFoundError("Organization's tenant not found");
        }

        var emailData = new
        {
            organization = tenant.Handle,
            expiration = tokenExpirationDays,
            userId = user.Id,
            token,
        };

        await _emailSender.SendAsync(user.Email, EmailTemplateKeys.AcceptInvitation, emailData, cancellationToken);
        return Result.Ok();
    }

    #endregion

}
