namespace NVOAMASIS.Services.MultiTenant;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        tenantContext.EnsureInitializedFromHttpContext();
        await next(context);
    }
}
