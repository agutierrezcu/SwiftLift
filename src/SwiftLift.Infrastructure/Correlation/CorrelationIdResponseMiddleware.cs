using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;
using Serilog.Context;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResponseMiddleware
    (ICorrelationIdResolver _correlationIdResolver, IFeatureManagerSnapshot _featureManager)
        : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = _correlationIdResolver.Resolve();

        var isAddCorrelationIdToResponseEnabled =
             await _featureManager.IsEnabledAsync("AddCorrelationIdToResponse")
              .ConfigureAwait(false);

        if (isAddCorrelationIdToResponseEnabled)
        {
            context.Response.OnStarting(() =>
            {
                var headerValue = new StringValues(correlationId.ToString() ?? "No set");

                context.Response.Headers.TryAdd(CorrelationIdHeader.Name, headerValue);

                return Task.CompletedTask;
            });
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context)
             .ConfigureAwait(false);
        }
    }
}
