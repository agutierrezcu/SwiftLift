using SwiftLift.SharedKernel.ConnectionString;
using SwiftLift.SharedKernel.Environment;

using static SwiftLift.SharedKernel.ApplicationInsight.ApplicationInsightSettings;

namespace SwiftLift.SharedKernel.ApplicationInsight;

public sealed class ApplicationInsightResource : IApplicationInsightResource
{
    public static readonly ApplicationInsightResource Instance = new();

    private ApplicationInsightResource()
    {
    }

    public ConnectionStringResource GetConnectionStringGuaranteed(
        IEnvironmentService environmentService, IConfiguration configuration)
    {
        Guard.Against.Null(environmentService);
        Guard.Against.Null(configuration);

        var connectionStringValue =
            environmentService.GetVariable(EnvironmentVariable)
            ?? configuration[EnvironmentVariable]
            ?? configuration[ConfigurationSectionKey];

        if (string.IsNullOrWhiteSpace(connectionStringValue))
        {
            throw new InvalidConnectionStringException(
                ResourceName,
                "Application Insight connection string can not be null or empty.");
        }

        ConnectionStringResource connectionStringResource;

        try
        {
            connectionStringResource =
                ConnectionStringParser.Parse(ResourceName, connectionStringValue);
        }
        catch (Exception ex)
        {
            throw new InvalidConnectionStringException(
                ResourceName,
                "Application Insight connection string is invalid.", ex);
        }

        if (!connectionStringResource.TryGetSegmentValue("InstrumentationKey", out _))
        {
            throw new InvalidConnectionStringException(
                ResourceName,
                "Application Insight connection string has no instrumentation key segment.");
        }

        return connectionStringResource;
    }
}
