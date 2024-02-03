using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace SwiftLift.Infrastructure.Logging;

internal sealed class SerilogLoggingActionFilter(IDiagnosticContext _diagnosticContext)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _diagnosticContext.Set("RouteData", context.ActionDescriptor.RouteValues);
        _diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
        _diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
        _diagnosticContext.Set("ValidationState", context.ModelState.IsValid);

        await next().ConfigureAwait(false);
    }
}
