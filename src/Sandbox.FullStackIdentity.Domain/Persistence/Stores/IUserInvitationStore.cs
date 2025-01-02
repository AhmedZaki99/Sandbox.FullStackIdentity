using Microsoft.AspNetCore.Identity;

namespace Sandbox.FullStackIdentity.Domain;

public interface IUserInvitationStore<TUser> : IUserStore<TUser> where TUser : class
{
    Task<bool> GetIsInvitedAsync(TUser user, CancellationToken cancellationToken);
    Task SetIsInvitedAsync(TUser user, bool isInvited, CancellationToken cancellationToken);

    Task<bool> GetInvitationAcceptedAsync(TUser user, CancellationToken cancellationToken);
    Task SetInvitationAcceptedAsync(TUser user, bool invitationAccepted, CancellationToken cancellationToken);
}
