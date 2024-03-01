using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;

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
                var headerValue = new StringValues(correlationId.ToString());

                context.Response.Headers.TryAdd(CorrelationIdHeader.Name, headerValue);

                return Task.CompletedTask;
            });
        }

        await next(context)
            .ConfigureAwait(false);
    }
}
