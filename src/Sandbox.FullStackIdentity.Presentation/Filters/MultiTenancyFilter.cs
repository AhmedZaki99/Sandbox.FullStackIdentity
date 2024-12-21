using Microsoft.AspNetCore.Mvc.Filters;
using Sandbox.FullStackIdentity.Domain;

namespace Sandbox.FullStackIdentity.Presentation;

public class MultiTenancyFilter : IAsyncActionFilter
{
    private readonly IMultiTenancyInitializer _multiTenancyInitializer;

    public MultiTenancyFilter(IMultiTenancyInitializer multiTenancyInitializer)
    {
        _multiTenancyInitializer = multiTenancyInitializer;
    }


    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantId = context.HttpContext.User.GetTenantId();
        _multiTenancyInitializer.SetCurrentTenant(tenantId);

        return next();
    }
}
