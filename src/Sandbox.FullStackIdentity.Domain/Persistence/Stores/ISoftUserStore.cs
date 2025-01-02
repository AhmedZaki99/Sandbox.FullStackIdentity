using Microsoft.AspNetCore.Identity;

namespace Sandbox.FullStackIdentity.Domain;

public interface ISoftUserStore<TUser> : IUserStore<TUser> where TUser : class
{
    Task<IdentityResult> DeletePermanentlyAsync(TUser user, CancellationToken cancellationToken = default);
}
