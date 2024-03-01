using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SwiftLift.Infrastructure.Configuration;
using SwiftLift.Infrastructure.ConnectionString;

namespace SwiftLift.Infrastructure.HealthChecks;

public static class HealthChecksWebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddHealthChecks(this WebApplicationBuilder builder,
      ConnectionStringResource applicationInsightConnectionString,
      params Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInsightConnectionString);
        Guard.Against.Null(applicationAssemblies);

        var isDevelopment = builder.Environment.IsDevelopment();

        var services = builder.Services;

        var healthChecksBuilder = services
            .AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
            .AddApplicationInsightsPublisher(
                connectionString: applicationInsightConnectionString.Value,
                saveDetailedReport: true,
                excludeHealthyReports: false);

        if (isDevelopment)
        {
            var seqServerUrl = builder.Configuration.GetRequired("SEQ_SERVER_URL")!;

            healthChecksBuilder
                .AddSeqPublisher(opts => opts.Endpoint = seqServerUrl);
        }

        services
            .Configure<HealthCheckPublisherOptions>(
                opts =>
                {
                    opts.Delay = TimeSpan.FromMinutes(2);
                    opts.Timeout = TimeSpan.FromSeconds(20);
                    opts.Period = TimeSpan.FromMinutes(isDevelopment ? 1 : 10);
                });

        return builder;
    }
}
