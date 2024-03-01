using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using Serilog.Context;

using static SwiftLift.Infrastructure.Correlation.CorrelationId;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResponseMiddleware
    (ICorrelationIdResolver _correlationIdResolver,
    IFeatureManagerSnapshot _featureManager,
    IHostEnvironment _hostEnvironment)
        : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = _correlationIdResolver.Resolve();

        if (_hostEnvironment.IsDevelopment() ||
                await _featureManager.IsEnabledAsync("AddCorrelationIdToResponse")
                    .ConfigureAwait(false))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.TryAdd(HeaderName, correlationId.ToString());

                return Task.CompletedTask;
            });
        }
        using (LogContext.PushProperty(LogEventPropertyName, correlationId))
        {
            await next(context)
                .ConfigureAwait(false);
        }
    }
}
