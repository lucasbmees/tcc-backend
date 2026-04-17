using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TccSharkTank.Application.Abstractions.Security;

namespace TccSharkTank.WebApi.Security;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal Principal => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public long? UserId
    {
        get
        {
            var value = Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Role => Principal.FindFirstValue(ClaimTypes.Role);
}

