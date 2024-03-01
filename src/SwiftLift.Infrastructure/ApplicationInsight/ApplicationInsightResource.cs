using SwiftLift.Infrastructure.ConnectionString;

using static SwiftLift.Infrastructure.ApplicationInsight.ApplicationInsightSettings;

namespace SwiftLift.Infrastructure.ApplicationInsight;

public sealed class ApplicationInsightResource : IApplicationInsightResource
{
    public static readonly ApplicationInsightResource Instance = new();

    private ApplicationInsightResource()
    {
    }

    public ConnectionStringResource GetConnectionStringGuaranteed(IConfiguration configuration)
    {
        Guard.Against.Null(configuration);

        var connectionStringValue =
            configuration[EnvironmentVariable] ??
                configuration[ConfigurationSectionKey];

        if (string.IsNullOrWhiteSpace(connectionStringValue))
        {
            throw new InvalidConnectionStringException(
                ResourceName,
                "Application Insight connection string can not be null or empty");
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
                "Application Insight connection string is invalid", ex);
        }

        if (!connectionStringResource.TryGetSegmentValue("InstrumentationKey", out _))
        {
            throw new InvalidConnectionStringException(
                ResourceName,
                "Application Insight connection string has no instrumentation key segment");
        }

        return connectionStringResource;
    }
}
