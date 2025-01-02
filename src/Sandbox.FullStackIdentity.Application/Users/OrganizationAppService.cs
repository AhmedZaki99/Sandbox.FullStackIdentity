using FluentResults;
using Hope.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.DependencyInjection;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Application;

/// <inheritdoc/>
internal sealed class OrganizationAppService : IOrganizationAppService
{

    #region Dependencies

    private readonly AppUserManager _userManager;
    private readonly IEmailCreator _emailCreator;
    private readonly ITenantHandleGenerator _tenantHandleGenerator;
    private readonly ITenantValidator _tenantValidator;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMultiTenancyContext _multiTenancyContext;
    private readonly ILogger<OrganizationAppService> _logger;

    public OrganizationAppService(
        IEmailCreator emailCreator,
        ITenantHandleGenerator tenantHandleGenerator,
        ITenantValidator tenantValidator,
        ITenantRepository tenantRepository,
        IMultiTenancyContext multiTenancyContext,
        ILogger<OrganizationAppService> logger,
        IServiceProvider serviceProvider)
    {
        // Resolve at runtime to make it possible to run the application without Identity service when not used.
        // Plus providing better exception message if the services are not registered when they are required.
        if (serviceProvider.GetService<AppUserManager>() is not AppUserManager appUserManager)
        {
            throw new InvalidOperationException(
                $"""
                The '{nameof(IOrganizationAppService)}' requires the '{nameof(AppUserManager)}' extension of the '{nameof(UserManager<User>)}'.
                Consider adding the Identity services followed by the '{nameof(ApplicationServiceCollectionExtensions.AddAppManagers)}' method.
                """);
        }

        _userManager = appUserManager;
        _emailCreator = emailCreator;
        _tenantHandleGenerator = tenantHandleGenerator;
        _tenantValidator = tenantValidator;
        _tenantRepository = tenantRepository;
        _multiTenancyContext = multiTenancyContext;
        _logger = logger;
    }

    #endregion

    #region Implementation

    /// <inheritdoc/>
    public async Task<PagedList<UserResponse>> ListUsersAsync(int page = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams(page, pageSize);
        var pageResult = await _tenantRepository.ListUsersAsync(paginationParams: paginationParams, cancellationToken: cancellationToken);

        return pageResult.MapItems(user => user.ToResponse());
    }


    /// <inheritdoc/>
    public async Task<Result<User>> CreateOrganizationAsync(RegisterRequest requestModel, CancellationToken cancellationToken = default)
    {
        var tenant = new Tenant
        {
            Name = requestModel.OrganizationName,
            Handle = await _tenantHandleGenerator.GenerateAsync(cancellationToken)
        };

        var validationResult = await _tenantValidator.ValidateAsync(tenant, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        var tenantResult = await _tenantRepository.CreateAsync(tenant, cancellationToken);
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
        await _emailCreator.CreateAsync(tenant.Handle, cancellationToken);

        _logger.LogInformation("Organization '{name}' with email '{email}' was created.", tenantResult.Value.Name, user.Email);
        return user;
    }
    
    /// <inheritdoc/>
    public async Task<Result<OrganizationResponse>> GenerateNewHandleAsync(CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);
        if (tenant is null)
        {
            return new NotFoundError("Organization not found.");
        }
        var oldHandle = tenant.Handle;
        var newHandle = await _tenantHandleGenerator.GenerateAsync(cancellationToken);

        var result = await _tenantRepository.ChangeHandleAsync(newHandle, cancellationToken: cancellationToken);
        if (result.IsFailed)
        {
            return result;
        }
        await _emailCreator.CreateAsync(newHandle, cancellationToken);
        await _emailCreator.DeleteAsync(oldHandle, cancellationToken);

        tenant.Handle = newHandle;
        return tenant.ToResponse();
    }
    
    /// <inheritdoc/>
    public async Task<Result<OrganizationResponse>> ChangeNameAsync(ChangeNameRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);
        if (tenant is null)
        {
            return new NotFoundError("Organization not found.");
        }
        tenant.Name = request.Name;

        var validationResult = await _tenantValidator.ValidateAsync(tenant, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        var changeResult = await _tenantRepository.ChangeNameAsync(request.Name, cancellationToken: cancellationToken);
        if (changeResult.IsFailed)
        {
            return changeResult;
        }
        return tenant.ToResponse();
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

        _logger.LogInformation("User '{email}' was invited to organization '{handle}'.", user.Email, tenant.Name);
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


    /// <inheritdoc/>
    public async Task<Result<OrganizationResponse>> AddEmailToBlacklistAsync(BlacklistRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);
        if (tenant is null)
        {
            return new NotFoundError("Organization not found.");
        }

        var result = await _tenantRepository.AddToBlacklistAsync(request.Emails, cancellationToken: cancellationToken);
        if (result.IsFailed)
        {
            return result.ToResult();
        }
        tenant.BlacklistedEmails = result.Value;

        return tenant.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<Result<OrganizationResponse>> RemoveEmailFromBlacklistAsync(BlacklistRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);
        if (tenant is null)
        {
            return new NotFoundError("Organization not found.");
        }

        var result = await _tenantRepository.RemoveFromBlacklistAsync(request.Emails, cancellationToken: cancellationToken);
        if (result.IsFailed)
        {
            return result.ToResult();
        }
        tenant.BlacklistedEmails = result.Value;

        return tenant.ToResponse();
    }

    /// <inheritdoc/>
    public async Task<Result<OrganizationResponse>> UpdateBlacklistAsync(BlacklistRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetAsync(cancellationToken: cancellationToken);
        if (tenant is null)
        {
            return new NotFoundError("Organization not found.");
        }

        var result = await _tenantRepository.UpdateBlacklistAsync(request.Emails, cancellationToken: cancellationToken);
        if (result.IsFailed)
        {
            return result.ToResult();
        }
        tenant.BlacklistedEmails = result.Value;

        return tenant.ToResponse();
    }

    #endregion

}
