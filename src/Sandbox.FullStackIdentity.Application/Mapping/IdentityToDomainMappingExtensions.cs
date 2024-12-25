using FluentResults;
using Hope.Results;
using Microsoft.AspNetCore.Identity;

namespace Sandbox.FullStackIdentity.Application;

public static class IdentityToDomainMappingExtensions
{
    public static Result ToFluentResult(this IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
        {
            return Result.Ok();
        }
        return Result.Fail(identityResult.Errors.Select(error => new ValidationError($"{error.Code}: {error.Description}")));
    }
}
