using Microsoft.AspNetCore.Http;

namespace SwiftLift.Infrastructure.UserContext;

internal sealed class UserContext(IHttpContextAccessor _httpContextAccessor)
    : IUserContext
{
    public bool TryGetUserId([NotNullWhen(true)] out string? userId)
    {
        userId = null;

        return _httpContextAccessor.HttpContext?.User?
            .TryGetUserId(out userId) ?? false;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?
            .Identity?.IsAuthenticated ?? false;
    }
}
