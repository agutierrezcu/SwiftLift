using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResponseMiddleware
    (ICorrelationIdResolver _correlationIdResolver, IFeatureManagerSnapshot _featureManager)
        : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var isAddCorrelationIdToResponseEnabled =
               await _featureManager.IsEnabledAsync("AddCorrelationIdToResponse")
                .ConfigureAwait(false);

        if (isAddCorrelationIdToResponseEnabled)
        {
            context.Response.OnStarting(() =>
            {
                if (!_correlationIdResolver.TryGet(out var correlationId))
                {
                    return Task.CompletedTask;
                }

                var headerValue = new StringValues(correlationId.ToString() ?? "No set");

                context.Response.Headers.TryAdd(CorrelationIdHeader.Name, headerValue);

                return Task.CompletedTask;
            });
        }

        await next(context)
          .ConfigureAwait(false);
    }
}
