using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.DependencyInjection;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <inheritdoc/>
internal sealed class UserAppService : IUserAppService
{

    #region Dependencies

    private readonly AppUserManager _userManager;
    private readonly ITenantRepository _tenantRepository;

    public UserAppService(
        ITenantRepository tenantRepository,
        IServiceProvider serviceProvider)
    {
        // Resolve at runtime to make it possible to run the application without Identity service when not used.
        // Plus providing better exception message if the services are not registered when they are required.
        if (serviceProvider.GetService<AppUserManager>() is not AppUserManager appUserManager)
        {
            throw new InvalidOperationException(
                $"""
                The '{nameof(IUserAppService)}' requires the '{nameof(AppUserManager)}' extension of the '{nameof(UserManager<User>)}'.
                Consider adding the Identity services followed by the '{nameof(ApplicationServiceCollectionExtensions.AddAppManagers)}' method.
                """);
        }

        _userManager = appUserManager;
        _tenantRepository = tenantRepository;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<UserDetailsResponse?> GetDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);

        return user.ToDetailsResponse(tenant);
    }

    /// <inheritdoc/>
    public async Task<LoginVerificationResult> VerifyLoginRequestAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return LoginVerificationResult.Fail("Invalid email or password.");
        }
        cancellationToken.ThrowIfCancellationRequested();

        if (!user.EmailConfirmed && (_userManager.Options.SignIn.RequireConfirmedAccount || _userManager.Options.SignIn.RequireConfirmedEmail))
        {
            return LoginVerificationResult.Fail("Email is not confirmed.");
        }

        bool userLockedOut = await _userManager.IsLockedOutAsync(user);
        if (userLockedOut)
        {
            return LoginVerificationResult.Fail("User is currently locked out.");
        }

        bool passwordCorrect = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordCorrect)
        {
            await _userManager.AccessFailedAsync(user);
            userLockedOut = await _userManager.IsLockedOutAsync(user);

            var errorMessage = userLockedOut
                ? "User is currently locked out."
                : "Invalid email or password.";

            return LoginVerificationResult.Fail(errorMessage);
        }
        await _userManager.ResetAccessFailedCountAsync(user);

        return LoginVerificationResult.Success(user);
    }

    public async Task<IdentityResult> CompleteInvitedUserAsync(User user, string token, string password, CancellationToken cancellationToken = default)
    {
        if (user.InvitationAccepted)
        {
            return IdentityResult.Failed(new IdentityError() { Description = "Invitation has already been accepted." });
        }
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _userManager.ConfirmInvitedEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return result;
        }
        return await _userManager.AddPasswordAsync(user, password);
    }

    #endregion

}
