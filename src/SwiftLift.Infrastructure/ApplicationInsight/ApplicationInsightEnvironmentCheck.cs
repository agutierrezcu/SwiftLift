using SwiftLift.Infrastructure.Checks;
using SwiftLift.Infrastructure.ConnectionString;

namespace SwiftLift.Infrastructure.ApplicationInsight;

[SkipCheck]
internal sealed class ApplicationInsightEnvironmentCheck : IEnvironmentCheck
{
    public string Description
        => "Application Insight connection string must be properly set";

    public Task Assert(IServiceProvider services, CancellationToken cancellation)
    {
        var applicationInsightResource = services.GetRequiredService<IApplicationInsightResource>();
        var configuration = services.GetRequiredService<IConfiguration>();

        var connectionStringResource = applicationInsightResource
            .GetConnectionStringGuaranteed(configuration);

        if (!connectionStringResource.TryGetSegmentValue("InstrumentationKey", out _))
        {
            throw new InvalidConnectionStringException(
                ApplicationInsightSettings.ResourceName,
                "Application Insight connection string does not have required InstrumentationKey segment key");
        }

        return Task.CompletedTask;
    }
}

