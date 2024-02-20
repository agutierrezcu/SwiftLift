using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using SwiftLift.Infrastructure.UserContext;

namespace SwiftLift.Infrastructure.Logging.Enrichers;

internal sealed class RequestUserIdEventEnricher(
    IHttpContextAccessor _httpContextAccessor, IUserContext _userContext)
        : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            return;
        }

        var isAuthenticated = _userContext.IsAuthenticated();

        AddRequestAuthenticatedProperty(logEvent, propertyFactory, isAuthenticated);

        if (!isAuthenticated)
        {
            return;
        }

        if (_userContext.TryGetUserId(out var userId))
        {
            AddRequestUserIdProperty(logEvent, propertyFactory, userId);
        }
        else
        {
            AddRequestUserIdProperty(logEvent, propertyFactory, "Unknow");
        }
    }

    private static void AddRequestAuthenticatedProperty(LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory, bool value)
    {
        var logEventProperty = propertyFactory.CreateProperty(
            "RequestAuthenticated", value);

        logEvent.AddPropertyIfAbsent(logEventProperty);
    }

    private static void AddRequestUserIdProperty(LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory, string userId)
    {
        var userIdProperty = propertyFactory.CreateProperty(
            "RequestUserId", userId);

        logEvent.AddPropertyIfAbsent(userIdProperty);
    }
}
