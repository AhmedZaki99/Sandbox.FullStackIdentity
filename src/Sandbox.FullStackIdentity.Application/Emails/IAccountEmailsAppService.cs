using FluentResults;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public interface IAccountEmailsAppService
{
    Task<Result> SendEmailConfirmationCodeAsync(User user, string code, CancellationToken cancellationToken = default);
    Task<Result> SendChangeEmailCodeAsync(User user, string code, string email, CancellationToken cancellationToken = default);
    Task<Result> SendTwoFactorCodeAsync(User user, string code, CancellationToken cancellationToken = default);

    Task<Result> SendResetPasswordLinkAsync(User user, string token, CancellationToken cancellationToken = default);
    Task<Result> SendInvitationLinkAsync(User user, string token, int codeExpirationDays, CancellationToken cancellationToken = default);
}
