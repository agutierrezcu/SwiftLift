using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;

namespace SwiftLift.Infrastructure.Logging;

public static class SerilogRequestLoggingOptions
{
    public static void Configure(RequestLoggingOptions options)
    {
        Guard.Against.Null(options);

        options.EnrichDiagnosticContext = EnrichFromHttpContext;
        options.GetLevel = GetLevel();
        options.IncludeQueryInRequestPath = true;
    }

    private static readonly HashSet<string> s_defaultExcludedEndpoints =
        [
            "Health checks",
            "Build info"
        ];

    public static Func<HttpContext, double, Exception?, LogEventLevel> GetLevel(
        LogEventLevel traceLevel = LogEventLevel.Verbose,
        params string[] traceEndpointNames)
    {
        Guard.Against.Null(traceEndpointNames);

        traceEndpointNames
            .ToList()
            .ForEach(t => s_defaultExcludedEndpoints.Add(t));

        return (ctx, _, ex) =>
            IsError(ctx, ex)
            ? LogEventLevel.Error
            : IsTraceEndpoint(ctx, s_defaultExcludedEndpoints)
                ? traceLevel
                : LogEventLevel.Information;
    }

    private static bool IsError(HttpContext ctx, Exception? ex)
    {
        return ex is not null || ctx.Response.StatusCode > 499;
    }

    private static bool IsTraceEndpoint(HttpContext ctx, ISet<string> traceEndpoints)
    {
        var endpoint = ctx.GetEndpoint();

        if (endpoint is null)
        {
            return false;
        }

        return traceEndpoints.Any(e => string.Equals(e, endpoint.DisplayName,
            StringComparison.OrdinalIgnoreCase));
    }

    public static void EnrichFromHttpContext(
        IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        Guard.Against.Null(diagnosticContext);
        Guard.Against.Null(httpContext);

        var request = httpContext.Request;

        diagnosticContext.Set("Host", request.Host);
        diagnosticContext.Set("Protocol", request.Protocol);
        diagnosticContext.Set("Scheme", request.Scheme);

        if (request.QueryString.HasValue)
        {
            diagnosticContext.Set("QueryString", request.QueryString.Value);
        }

        var response = httpContext.Response;

        diagnosticContext.Set("ContentType", response.ContentType);

        var endpoint = httpContext.GetEndpoint();

        if (endpoint is not null)
        {
            diagnosticContext.Set("EndpointName", endpoint.DisplayName);
        }
    }
}
