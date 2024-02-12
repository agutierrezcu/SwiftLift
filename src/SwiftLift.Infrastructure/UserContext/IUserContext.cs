namespace SwiftLift.Infrastructure.UserContext;

public interface IUserContext
{
    bool IsAuthenticated();

    bool TryGetUserId([NotNullWhen(true)] out string? userId);
}
