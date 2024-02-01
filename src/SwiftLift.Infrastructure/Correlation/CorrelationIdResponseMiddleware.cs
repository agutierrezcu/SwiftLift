using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResponseMiddleware(
    ICorrelationIdResolver correlationIdResolver,
    IFeatureManagerSnapshot featureManager)
        : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var isAddCorrelationIdToResponseEnabled =
               featureManager.IsEnabledAsync("AddCorrelationIdToResponse")
                   .GetAwaiter().GetResult();

        if (isAddCorrelationIdToResponseEnabled)
        {
            context.Response.OnStarting(() =>
            {
                _ = correlationIdResolver.TryGet(out var correlationId);

                var headerValue = new StringValues(correlationId.ToString() ?? "No set");

                context.Response.Headers.TryAdd(CorrelationIdHeader.Name, headerValue);

                return Task.CompletedTask;
            });
        }

        await next(context)
          .ConfigureAwait(false);
    }
}
