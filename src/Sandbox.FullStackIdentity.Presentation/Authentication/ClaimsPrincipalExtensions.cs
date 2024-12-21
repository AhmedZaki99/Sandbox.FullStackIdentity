using System.Security.Claims;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.GetGuidClaim(ClaimTypes.NameIdentifier);
    }

    public static Guid? GetTenantId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.GetGuidClaim(ApplicationClaimTypes.TenantId);
    }


    public static Guid? GetGuidClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        var guidClaim = claimsPrincipal.FindFirstValue(claimType);
        if (guidClaim is not null && Guid.TryParse(guidClaim, out var guid))
        {
            return guid;
        }
        return null;
    }

    public static bool? GetBooleanClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        var booleanClaim = claimsPrincipal.FindFirstValue(claimType);
        if (booleanClaim is not null && bool.TryParse(booleanClaim, out var boolean))
        {
            return boolean;
        }
        return null;
    }

    public static T? GetEnumClaim<T>(this ClaimsPrincipal claimsPrincipal, string claimType) where T : struct
    {
        var enumClaim = claimsPrincipal.FindFirstValue(claimType);
        if (enumClaim is not null && Enum.TryParse<T>(enumClaim, out var enumValue))
        {
            return enumValue;
        }
        return null;
    }
}
