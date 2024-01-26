using Oakton.Environment;
using SwiftLift.SharedKernel.ConnectionString;
using SwiftLift.SharedKernel.Environment;

namespace SwiftLift.SharedKernel.ApplicationInsight;

internal sealed class ApplicationInsightEnvironmentCheck : IEnvironmentCheck
{
    public string Description
        => "Application Insight connection string must be properly set.";

    public Task Assert(IServiceProvider services, CancellationToken cancellation)
    {
        var applicationInsightResource = services.GetRequiredService<IApplicationInsightResource>();
        var environmentService = services.GetRequiredService<IEnvironmentService>();
        var configuration = services.GetRequiredService<IConfiguration>();

        var connectionStringResource = applicationInsightResource
            .GetConnectionStringGuaranteed(
                environmentService, configuration);

        if (!connectionStringResource?.TryGetSegmentValue("InstrumentationKey", out _) ?? true)
        {
            throw new InvalidConnectionStringException(
                ApplicationInsightSettings.ResourceName,
                "Application Insight connection string does not have required InstrumentationKey segment key.");
        }

        return Task.CompletedTask;
    }
}

