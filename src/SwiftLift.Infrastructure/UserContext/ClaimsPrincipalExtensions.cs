using System.Security.Claims;

namespace SwiftLift.Infrastructure.UserContext;

internal static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal? principal, [NotNullWhen(true)] out string? userId)
    {
        userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknow";

        return string.IsNullOrWhiteSpace(userId);
    }
}
