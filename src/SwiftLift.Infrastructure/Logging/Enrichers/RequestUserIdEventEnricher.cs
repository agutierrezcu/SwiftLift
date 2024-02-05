using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.Logging.Enrichers;

internal sealed class RequestUserIdEventEnricher(IHttpContextAccessor _httpContextAccessor)
    : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Guard.Against.Null(logEvent);
        Guard.Against.Null(propertyFactory);

        if (_httpContextAccessor.HttpContext is null)
        {
            return;
        }

        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated ?? false)
        {
            RequestAuthenticatedProperty(logEvent, propertyFactory, true);

            var userIdProperty = propertyFactory.CreateProperty(
                "RequestUserId", user?.Identity?.Name ?? "Unknow");

            logEvent.AddPropertyIfAbsent(userIdProperty);

            return;
        }

        RequestAuthenticatedProperty(logEvent, propertyFactory, false);
    }

    private static void RequestAuthenticatedProperty(LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory, bool value)
    {
        var logEventProperty = propertyFactory.CreateProperty(
            "RequestAuthenticated", value);

        logEvent.AddPropertyIfAbsent(logEventProperty);
    }
}
