using Microsoft.AspNetCore.Builder;

namespace SwiftLift.Infrastructure.Correlation;

internal static class CorrelationIdResponseMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationIdResponseMiddleware(this IApplicationBuilder app)
    {
        Guard.Against.Null(app);

        return app.UseMiddleware<CorrelationIdResponseMiddleware>();
    }
}
