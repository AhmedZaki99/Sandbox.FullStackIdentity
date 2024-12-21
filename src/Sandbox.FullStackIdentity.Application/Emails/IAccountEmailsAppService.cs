using FluentResults;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public interface IAccountEmailsAppService
{
    Task<Result> SendEmailConfirmationCodeAsync(User user, string code, CancellationToken cancellationToken = default);
    Task<Result> SendInvitationLinkAsync(User user, string linkPath, string code, int codeExpirationDays, CancellationToken cancellationToken = default);
}
