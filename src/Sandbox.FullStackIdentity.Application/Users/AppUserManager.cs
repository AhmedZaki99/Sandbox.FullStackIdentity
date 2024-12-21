using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

public class AppUserManager : UserManager<User>
{

    #region Constants
    
    public const string InvitationTokenPurpose = "Invitation";

    #endregion

    #region Dependencies

    public AppUserManager(
        IUserStore<User> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<User> passwordHasher,
        IEnumerable<IUserValidator<User>> userValidators,
        IEnumerable<IPasswordValidator<User>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<User>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    #endregion

    #region Implementation

    public virtual bool SupportsSoftDelete
    {
        get
        {
            ThrowIfDisposed();
            return Store is ISoftUserStore<User>;
        }
    }


    public virtual Task<IdentityResult> DeletePermanentlyAsync(User user)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        return GetSoftUserStore().DeletePermanentlyAsync(user, CancellationToken);
    }

    public virtual Task<string> GenerateInvitationTokenAsync(User user)
    {
        return GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, InvitationTokenPurpose);
    }

    public virtual async Task<bool> VerifyInvitationTokenAsync(User user, string token)
    {
        return await VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, InvitationTokenPurpose, token);
    }

    public virtual async Task<IdentityResult> ConfirmInvitedEmailAsync(User user, string token)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        if (Store is not IUserEmailStore<User> emailStore)
        {
            throw new NotSupportedException("The user store doesn't support email confirmation.");
        }

        if (!await VerifyInvitationTokenAsync(user, token))
        {
            return IdentityResult.Failed(ErrorDescriber.InvalidToken());
        }
        await emailStore.SetEmailConfirmedAsync(user, true, CancellationToken);
        return await UpdateUserAsync(user);
    }

    #endregion

    #region Helpers

    private ISoftUserStore<User> GetSoftUserStore()
    {
        return Store as ISoftUserStore<User> 
            ?? throw new NotSupportedException("The user store service doesn't support soft deletions.");
    }

    #endregion

}
