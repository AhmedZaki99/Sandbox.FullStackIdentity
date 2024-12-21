using FluentResults;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.DependencyInjection;
using Sandbox.FullStackIdentity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Sandbox.FullStackIdentity.Application;

/// <inheritdoc/>
internal sealed class OrganizationAppService : IOrganizationAppService
{

    #region Dependencies

    private readonly AppUserManager _userManager;
    private readonly ITenantValidator _tenantValidator;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMultiTenancyContext _multiTenancyContext;
    private readonly ILogger<OrganizationAppService> _logger;

    public OrganizationAppService(
        UserManager<User> userManager,
        ITenantValidator tenantValidator,
        ITenantRepository tenantRepository,
        IMultiTenancyContext multiTenancyContext,
        ILogger<OrganizationAppService> logger)
    {
        if (userManager is not AppUserManager appUserManager)
        {
            throw new InvalidOperationException(
                $"""
                The '{nameof(IOrganizationAppService)}' requires the '{nameof(AppUserManager)}' implementation of the '{nameof(UserManager<User>)}'.
                Consider adding the Identity services followed by the '{nameof(ApplicationServiceCollectionExtensions.AddAppManagers)}' method.
                """);
        }

        _userManager = appUserManager;
        _tenantValidator = tenantValidator;
        _tenantRepository = tenantRepository;
        _multiTenancyContext = multiTenancyContext;
        _logger = logger;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<Result<User>> CreateOrganizationAsync(RegisterRequest requestModel, CancellationToken cancellationToken = default)
    {
        var validationResult = await _tenantValidator.ValidateAsync(requestModel.OrganizationHandle, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        var tenantResult = await _tenantRepository.CreateAsync(requestModel.OrganizationHandle, requestModel.OrganizationName, cancellationToken);
        if (tenantResult.IsFailed)
        {
            return tenantResult.ToResult();
        }

        var user = new User
        {
            TenantId = tenantResult.Value.Id,
            UserName = requestModel.Email,
            Email = requestModel.Email
        };
        var userResult = await _userManager.CreateAsync(user, requestModel.Password);
        if (!userResult.Succeeded)
        {
            // Cleanup for future retries.
            await _tenantRepository.DeletePermanentlyAsync(tenantResult.Value.Id, cancellationToken);

            return userResult.ToFluentResult();
        }

        var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.Organization);
        if (!roleResult.Succeeded)
        {
            // Cleanup for future retries.
            await _userManager.DeletePermanentlyAsync(user);
            await _tenantRepository.DeletePermanentlyAsync(tenantResult.Value.Id, cancellationToken);

            return roleResult.ToFluentResult();
        }

        _logger.LogInformation("Organization '{handle}' with email '{email}' was created.", tenantResult.Value.Handle, user.Email);
        return user;
    }


    /// <inheritdoc/>
    public async Task<Result<User>> CreateInvitedUserAsync(InviteRequest requestModel, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);
        if (tenant is null)
        {
            return new NotFoundError("Organization not found.");
        }

        var user = new User
        {
            TenantId = tenant.Id,
            UserName = requestModel.Email,
            Email = requestModel.Email,
            GrantedPermission = requestModel.GivenPermission,
            IsInvited = true,
            InvitationAccepted = false
        };
        var userResult = await _userManager.CreateAsync(user);
        if (!userResult.Succeeded)
        {
            return userResult.ToFluentResult();
        }

        _logger.LogInformation("User '{email}' was invited to organization '{handle}'.", user.Email, tenant.Handle);
        return user;
    }

    /// <inheritdoc/>
    public async Task<Result> RemoveInvitedUserAsync(InvitedUserRequest requestModel, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(requestModel.Email);
        if (user is null)
        {
            return new NotFoundError("User not found.");
        }

        if (!user.IsInvited || user.TenantId != _multiTenancyContext.CurrentTenantId)
        {
            return new ConflictError("User is not invited by your organization.");
        }
        if (user.InvitationAccepted)
        {
            return new ConflictError("User has already accepted the invitation.");
        }

        var result = await _userManager.DeletePermanentlyAsync(user);
        if (!result.Succeeded)
        {
            return result.ToFluentResult();
        }

        _logger.LogInformation("Invitation for user '{email}' was removed.", user.Email);
        return Result.Ok();
    }
    
    /// <inheritdoc/>
    public async Task<Result> ChangeUserPermissionsAsync(InvitedUserRequest requestModel, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(requestModel.Email);
        if (user is null)
        {
            return new NotFoundError("User not found.");
        }

        if (!user.IsInvited || user.TenantId != _multiTenancyContext.CurrentTenantId)
        {
            return new ConflictError("User is not invited by your organization.");
        }
        user.GrantedPermission = requestModel.GivenPermission;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return result.ToFluentResult();
        }
        return Result.Ok();
    }

    #endregion

}
