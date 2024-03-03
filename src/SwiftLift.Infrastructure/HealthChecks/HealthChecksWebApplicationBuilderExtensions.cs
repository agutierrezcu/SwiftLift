using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SwiftLift.Infrastructure.Configuration;

namespace SwiftLift.Infrastructure.HealthChecks;

public static class HealthChecksWebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddHealthChecks(this WebApplicationBuilder builder,
      params Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrEmpty(applicationAssemblies);

        var services = builder.Services;

        var healthChecksBuilder =
            services
                .AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        var isDevelopment = builder.Environment.IsDevelopment();

        if (isDevelopment)
        {
            var seqServerUrl = builder.Configuration.GetRequiredValue("SEQ_SERVER_URL")!;

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
