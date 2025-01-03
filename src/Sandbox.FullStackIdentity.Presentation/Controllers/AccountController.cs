using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Contracts;

namespace Sandbox.FullStackIdentity.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController : ControllerBase
{

    #region Dependencies

    private readonly AppUserManager _userManager;
    private readonly IUserAppService _userAppService;
    private readonly IAccountEmailsAppService _accountEmailsAppService;
    private readonly IBearerTokenGenerator _bearerTokenGenerator;

    public AccountController(
        AppUserManager userManager,
        IUserAppService userAppService,
        IAccountEmailsAppService accountEmailsAppService,
        IBearerTokenGenerator bearerTokenGenerator)
    {
        _userManager = userManager;
        _userAppService = userAppService;
        _accountEmailsAppService = accountEmailsAppService;
        _bearerTokenGenerator = bearerTokenGenerator;
    }

    #endregion

    #region Data Actions

    [HttpGet]
    public async Task<ActionResult<UserDetailsResponse>> GetDetails(CancellationToken cancellationToken)
    {
        var userId = User.GetId();
        var userDetails = userId is null
            ? null
            : await _userAppService.GetDetailsAsync(userId.Value, cancellationToken);

        if (userDetails is null)
        {
            return Unauthorized("Invalid or expired bearer token.");
        }
        return userDetails;
    }

    #endregion

    #region Login Actions

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<BearerTokenResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var verificationResult = await _userAppService.VerifyLoginRequestAsync(request, cancellationToken);
        if (!verificationResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, verificationResult.ErrorMessage);
            return ValidationProblem(ModelState);
        }
        
        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(verificationResult.User, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return tokenResponse;
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] UserEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.EmailConfirmed)
        {
            // Don't reveal private information to anonymous users.
            return Ok();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        await _accountEmailsAppService.SendResetPasswordLinkAsync(user, token, cancellationToken);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{request.UserId}'.");
        }
        cancellationToken.ThrowIfCancellationRequested();

        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

        var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return ValidationProblem(ModelState);
        }
        return Ok();
    }

    #endregion

    #region Registration Actions

    [AllowAnonymous]
    [HttpPost("resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] UserEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.EmailConfirmed)
        {
            // Don't reveal private information to anonymous users.
            return Ok();
        }
        cancellationToken.ThrowIfCancellationRequested();

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        await _accountEmailsAppService.SendEmailConfirmationCodeAsync(user, code, cancellationToken);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<ActionResult<BearerTokenResponse>> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{request.UserId}'.");
        }

        if (user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "Email has already been confirmed.");
            return ValidationProblem(ModelState);
        }
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _userManager.ConfirmEmailAsync(user, request.Code);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return tokenResponse;
    }
    
    [AllowAnonymous]
    [HttpPost("accept-invitation")]
    public async Task<ActionResult<BearerTokenResponse>> AcceptInvitation([FromBody] AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{request.UserId}'.");
        }
        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

        var result = await _userAppService.CompleteInvitedUserAsync(user, token, request.Password, cancellationToken);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return tokenResponse;
    }

    #endregion

    #region Management Actions

    [HttpPost("change-password")]
    public async Task<ActionResult<BearerTokenResponse>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized("Invalid or expired bearer token.");
        }
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens after successful password change.");
        }
        return tokenResponse;
    }

    [HttpPost("change-email")]
    public async Task<ActionResult<ChangeEmailResponse>> ChangeEmail([FromBody] ChangeEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized("Invalid or expired bearer token.");
        }
        if (user.Email == request.NewEmail)
        {
            ModelState.AddModelError(string.Empty, "New email is the same as the current email.");
            return ValidationProblem(ModelState);
        }
        cancellationToken.ThrowIfCancellationRequested();

        var code = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
        await _accountEmailsAppService.SendChangeEmailCodeAsync(user, code, request.NewEmail, cancellationToken);

        return new ChangeEmailResponse(user.Id, request.NewEmail, Status: "Email confirmation code has been sent.");
    }

    [AllowAnonymous]
    [HttpPost("confirm-email-change")]
    public async Task<ActionResult<BearerTokenResponse>> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{request.UserId}'.");
        }
        cancellationToken.ThrowIfCancellationRequested();

        var result = await _userManager.ChangeEmailAsync(user, request.Email, request.Code);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        // Username and Email are the same.
        result = await _userManager.SetUserNameAsync(user, request.Email);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var tokenResponse = await _bearerTokenGenerator.GenerateAsync(user, cancellationToken);
        if (tokenResponse is null)
        {
            return Problem("Failed to generate authentication tokens.");
        }
        return tokenResponse;
    }
    
    #endregion

}
