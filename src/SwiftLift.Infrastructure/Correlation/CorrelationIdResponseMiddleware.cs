using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResponseMiddleware(RequestDelegate _next)
{
    public async Task InvokeAsync(HttpContext context,
        ICorrelationIdResolver correlationIdResolver, IFeatureManagerSnapshot featureManager)
    {
        var isAddCorrelationIdToResponseEnabled =
               await featureManager.IsEnabledAsync("AddCorrelationIdToResponse")
                .ConfigureAwait(false);

        if (isAddCorrelationIdToResponseEnabled)
        {
            context.Response.OnStarting(() =>
            {
                if (!correlationIdResolver.TryGet(out var correlationId))
                {
                    return Task.CompletedTask;
                }

                var headerValue = new StringValues(correlationId.ToString() ?? "No set");

                context.Response.Headers.TryAdd(CorrelationIdHeader.Name, headerValue);

                return Task.CompletedTask;
            });
        }

        await _next(context)
          .ConfigureAwait(false);
    }
}
