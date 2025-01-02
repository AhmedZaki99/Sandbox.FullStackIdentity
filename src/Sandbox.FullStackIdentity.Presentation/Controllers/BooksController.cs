using System.Security.Claims;
using Hope.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sandbox.FullStackIdentity.Application;
using Sandbox.FullStackIdentity.Contracts;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/books")]
public sealed class BooksController : ControllerBase
{

    #region Dependencies

    private readonly IBookAppService _bookAppService;

    public BooksController(IBookAppService bookAppService)
    {
        _bookAppService = bookAppService;
    }

    #endregion

    #region Actions

    [HttpGet]
    public async Task<ActionResult<PagedList<BookResponse>>> List(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 30, 
        [FromQuery] Guid creatorId = default,
        [FromQuery] string? senderEmail = null, 
        CancellationToken cancellationToken = default)
    {
        if (!ReadPermissionGranted(User))
        {
            return Unauthorized("You don't have permission to read your organization's data.");
        }

        if (creatorId != default)
        {
            return await _bookAppService.ListAsync(creatorId, page, pageSize, cancellationToken);
        }
        if (senderEmail != null)
        {
            return await _bookAppService.ListAsync(senderEmail, page, pageSize, cancellationToken);
        }
        return await _bookAppService.ListAsync(page, pageSize, cancellationToken);
    }

    [HttpGet("deleted")]
    public async Task<ActionResult<PagedList<BookResponse>>> ListDeleted([FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken cancellationToken = default)
    {
        if (!ReadPermissionGranted(User))
        {
            return Unauthorized("You don't have permission to read your organization's data.");
        }
        return await _bookAppService.ListDeletedAsync(page, pageSize, cancellationToken);
    }


    [HttpGet("{bookId:guid}")]
    public async Task<ActionResult<BookResponse>> Get(Guid bookId, CancellationToken cancellationToken = default)
    {
        if (!ReadPermissionGranted(User))
        {
            return Unauthorized("You don't have permission to read your organization's data.");
        }

        var book = await _bookAppService.GetAsync(bookId, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        return book;
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create([FromBody] BookRequest request, CancellationToken cancellationToken = default)
    {
        if (!WritePermissionGranted(User))
        {
            return Unauthorized("You don't have permission to write to your organization's data.");
        }
        var userId = User.GetId();
        if (userId is null)
        {
            return Unauthorized("Invalid or expired bearer token.");
        }

        var result = await _bookAppService.CreateAsync(request, userId.Value, cancellationToken);
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

    [HttpPatch("{bookId:guid}")]
    public async Task<ActionResult<BookResponse>> Update(Guid bookId, [FromBody] BookRequest request, CancellationToken cancellationToken = default)
    {
        if (!WritePermissionGranted(User))
        {
            return Unauthorized("You don't have permission to write to your organization's data.");
        }

        var result = await _bookAppService.UpdateAsync(bookId, request, cancellationToken);
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

    [HttpDelete("{bookId:guid}")]
    public async Task<IActionResult> Delete(Guid bookId, CancellationToken cancellationToken = default)
    {
        if (!WritePermissionGranted(User))
        {
            return Unauthorized("You don't have permission to write to your organization's data.");
        }

        var result = await _bookAppService.DeleteAsync(bookId, cancellationToken);
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

    #endregion

    #region Helpers

    private static bool ReadPermissionGranted(ClaimsPrincipal claimsPrincipal)
    {
        var isOrganization = claimsPrincipal.IsInRole(ApplicationRoles.Organization);
        if (isOrganization)
        {
            return true;
        }
        var grantedPermission = claimsPrincipal.GetEnumClaim<OrganizationPermission>(ApplicationClaimTypes.GrantedPermission);

        return grantedPermission is OrganizationPermission.ReadWrite;
    }
    
    private static bool WritePermissionGranted(ClaimsPrincipal claimsPrincipal)
    {
        var isOrganization = claimsPrincipal.IsInRole(ApplicationRoles.Organization);
        if (isOrganization)
        {
            return true;
        }
        var grantedPermission = claimsPrincipal.GetEnumClaim<OrganizationPermission>(ApplicationClaimTypes.GrantedPermission);

        return grantedPermission is OrganizationPermission.WriteOnly or OrganizationPermission.ReadWrite;
    }

    #endregion

}
