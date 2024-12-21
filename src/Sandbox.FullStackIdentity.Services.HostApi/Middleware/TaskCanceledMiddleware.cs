namespace Sandbox.FullStackIdentity.Services.HostApi;

public class TaskCanceledMiddleware
{

    private readonly RequestDelegate _next;

    public TaskCanceledMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            // Set StatusCode 499 Client Closed Request
            context.Response.StatusCode = 499;
        }
    }
}