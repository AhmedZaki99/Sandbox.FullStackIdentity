using FluentResults;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public interface IAccountEmailsAppService
{
    Task<Result> SendEmailConfirmationCodeAsync(User user, string code, CancellationToken cancellationToken = default);

    Task<Result> SendResetPasswordLinkAsync(User user, string token, CancellationToken cancellationToken = default);
    Task<Result> SendChangeEmailLinkAsync(User user, string token, string email, CancellationToken cancellationToken = default);

    Task<Result> SendInvitationLinkAsync(User user, string token, int codeExpirationDays, CancellationToken cancellationToken = default);
}
