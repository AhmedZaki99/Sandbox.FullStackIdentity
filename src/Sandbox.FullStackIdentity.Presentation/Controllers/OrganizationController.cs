using System.Text;
using FluentResults;
using Hope.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Organization)]
[Route("api/organization")]
public sealed class OrganizationController : ControllerBase
{

    #region Dependencies

    private readonly AppUserManager _userManager;
    private readonly IBearerTokenGenerator _bearerTokenGenerator;
    private readonly IOrganizationAppService _organizationAppService;
    private readonly IAccountEmailsAppService _accountEmailsAppService;
    private readonly IOptions<DataProtectionTokenProviderOptions> _dataProtectionTokenProviderOptions;

    public OrganizationController(
        AppUserManager userManager,
        IBearerTokenGenerator bearerTokenGenerator,
        IOrganizationAppService organizationAppService,
        IAccountEmailsAppService accountEmailsAppService,
        IOptions<DataProtectionTokenProviderOptions> dataProtectionTokenProviderOptions)
    {
        _userManager = userManager;
        _bearerTokenGenerator = bearerTokenGenerator;
        _organizationAppService = organizationAppService;
        _accountEmailsAppService = accountEmailsAppService;
        _dataProtectionTokenProviderOptions = dataProtectionTokenProviderOptions;
    }

    #endregion

    #region Actions

    [HttpGet("users")]
    public async Task<ActionResult<PagedList<UserResponse>>> GetUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 30, 
        CancellationToken cancellationToken = default)
    {
        return await _organizationAppService.ListUsersAsync(page, pageSize, cancellationToken);
    }


    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return BadRequest("User is already logged in.");
        }

        var result = await _organizationAppService.CreateOrganizationAsync(request, cancellationToken);
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.GetName(), error.Message);
            }
            return ValidationProblem(ModelState);
        }

        var user = result.Value;
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        await _accountEmailsAppService.SendEmailConfirmationCodeAsync(user, code, cancellationToken);

        if (_userManager.Options.SignIn.RequireConfirmedAccount || _userManager.Options.SignIn.RequireConfirmedEmail)
        {
            var registerResponse = new RegisterResponse(user.Id, user.Email, Status: "Email confirmation code has been sent.");
            return Ok(registerResponse);
        }
        
        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return Ok(tokenResponse);
    }


    [HttpPost("invite")]
    public async Task<IActionResult> Invite([FromBody] InviteRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.CreateInvitedUserAsync(request, cancellationToken);
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.GetName(), error.Message);
            }
            return ValidationProblem(ModelState);
        }
        var user = result.Value;

        var token = await _userManager.GenerateInvitationTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var codeExpirationDays = (int)Math.Floor(_dataProtectionTokenProviderOptions.Value.TokenLifespan.TotalDays);

        await _accountEmailsAppService.SendInvitationLinkAsync(user, token, codeExpirationDays, cancellationToken);
        return Ok();
    }

    [HttpPost("cancel-invitation")]
    public async Task<IActionResult> CancelInvitation([FromBody] InvitedUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.RemoveInvitedUserAsync(request, cancellationToken);
        return GetSuccessOrValidationProblem(result);
    }

    [HttpPost("change-permissions")]
    public async Task<IActionResult> ChangePermissions([FromBody] InvitedUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.ChangeUserPermissionsAsync(request, cancellationToken);
        return GetSuccessOrValidationProblem(result);
    }
    

    [HttpPost("generate-email")]
    public async Task<ActionResult<OrganizationResponse>> GenerateEmail(CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.GenerateNewHandleAsync(cancellationToken);
        return GetValueOrValidationProblem(result);
    }

    [HttpPost("change-name")]
    public async Task<ActionResult<OrganizationResponse>> ChangeName(ChangeNameRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.ChangeNameAsync(request, cancellationToken);
        return GetValueOrValidationProblem(result);
    }


    [HttpPost("add-to-blacklist")]
    public async Task<ActionResult<OrganizationResponse>> AddToBlacklist(BlacklistRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.AddEmailToBlacklistAsync(request, cancellationToken);
        return GetValueOrValidationProblem(result);
    }

    [HttpPost("remove-from-blacklist")]
    public async Task<ActionResult<OrganizationResponse>> RemoveFromBlacklist(BlacklistRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.RemoveEmailFromBlacklistAsync(request, cancellationToken);
        return GetValueOrValidationProblem(result);
    }

    [HttpPost("update-blacklist")]
    public async Task<ActionResult<OrganizationResponse>> UpdateBlacklist(BlacklistRequest request, CancellationToken cancellationToken)
    {
        var result = await _organizationAppService.UpdateBlacklistAsync(request, cancellationToken);
        return GetValueOrValidationProblem(result);
    }

    #endregion

    #region Helpers

    private IActionResult GetSuccessOrValidationProblem(Result result)
    {
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.GetName(), error.Message);
            }
            return ValidationProblem(ModelState);
        }
        return Ok();
    }
    
    private ActionResult<T> GetValueOrValidationProblem<T>(Result<T> result)
    {
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.GetName(), error.Message);
            }
            return ValidationProblem(ModelState);
        }
        return result.Value;
    }

    #endregion

}
