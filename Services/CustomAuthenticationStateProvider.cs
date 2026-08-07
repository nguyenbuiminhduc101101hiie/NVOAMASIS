using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace NVOAMASIS.Services;

/// <summary>
/// Blazor InteractiveServer auth state for IIS / circuit scenarios.
/// Prefer host-provided authentication state; fall back to HttpContext.User only when available.
/// HttpContext is often null after circuit start under IIS — relying on it alone causes login→logout.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IHostEnvironmentAuthenticationStateProvider
{
    private readonly IHttpContextAccessor _http;
    private Task<AuthenticationState>? _authenticationStateTask;

    public CustomAuthenticationStateProvider(IHttpContextAccessor http)
    {
        _http = http;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authenticationStateTask != null)
            return _authenticationStateTask;

        var user = _http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
            return Task.FromResult(new AuthenticationState(user));

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    public void SetAuthenticationState(Task<AuthenticationState> authenticationStateTask)
    {
        _authenticationStateTask = authenticationStateTask
            ?? throw new ArgumentNullException(nameof(authenticationStateTask));
        NotifyAuthenticationStateChanged(_authenticationStateTask);
    }

    public Task<AuthenticationState> GetAuth() => GetAuthenticationStateAsync();
}
