using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseCorrelationIdResponseMiddleware();

            next(app);
        };
    }
}
