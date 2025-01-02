using Microsoft.AspNetCore.Identity;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public interface IUserAppService
{
    Task<UserDetailsResponse?> GetDetailsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<LoginVerificationResult> VerifyLoginRequestAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<IdentityResult> CompleteInvitedUserAsync(User user, string token, string password, CancellationToken cancellationToken = default);
}
