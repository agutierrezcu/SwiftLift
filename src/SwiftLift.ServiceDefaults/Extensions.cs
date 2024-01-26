using System.Reflection;
using Ardalis.GuardClauses;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SwiftLift.SharedKernel.Application;
using SwiftLift.Infrastructure.Build;

namespace SwiftLift.ServiceDefaults;

public static partial class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder,
        ApplicationInfo applicationInfo, Assembly[] assemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInfo);
        Guard.Against.NullOrEmpty(assemblies);

        builder.AddSharedServices(assemblies);

        builder.ConfigureOpenTelemetry(applicationInfo);

        builder.AddDefaultHealthChecks();

        var services = builder.Services;

        services.AddServiceDiscovery();

        services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.UseServiceDiscovery();
        });

        builder.AddEnvironmentChecks(assemblies);

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder,
        ApplicationInfo applicationInfo)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInfo);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                       .AddBuiltInMeters();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // We want to view all traces in development
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddAspNetCoreInstrumentation()
                       .AddGrpcClientInstrumentation()
                       .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters(applicationInfo);

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder,
        ApplicationInfo applicationInfo)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInfo);

        var services = builder.Services;

        var otlpExporterEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        var useOtlpExporter = !string.IsNullOrWhiteSpace(otlpExporterEndpoint);

        if (useOtlpExporter)
        {
            services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
            services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        }

        // Uncomment the following lines to enable the Prometheus exporter (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
        // builder.Services.AddOpenTelemetry()
        //    .WithMetrics(metrics => metrics.AddPrometheusExporter());

        var resourceAttributes = new Dictionary<string, object> {
            { "service.name", applicationInfo.Name },
            { "service.namespace", applicationInfo.Namespace },
            { "service.instance.id", applicationInfo.Id }
        };

        services.ConfigureOpenTelemetryTracerProvider(
            builder => builder.ConfigureResource(
                resourceBuilder => resourceBuilder.AddAttributes(resourceAttributes)));

        // TODO: check this out later having into account the issue https://github.com/dotnet/aspire/issues/1562
        services.AddOpenTelemetry()
           .UseAzureMonitor(
                opts =>
                {
                    opts.SamplingRatio = 0.05F;

                    opts.Credential =
                        builder.Environment.IsDevelopment()
                        ? new DefaultAzureCredential()
                        : new ManagedIdentityCredential();
                }
            );

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Uncomment the following line to enable the Prometheus endpoint (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
        // app.MapPrometheusScrapingEndpoint();

        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        app.MapGet("/build-info",
            async (IBuildInfoManager buildInfoManager, HttpResponse response, CancellationToken cancellation) =>
            {
                var fileContent =
                    await buildInfoManager.GetBuildInfoAsStringAsync(cancellation)
                        .ConfigureAwait(false);

                response.ContentType = "application/json; charset=utf-8";

                await response.WriteAsync(fileContent, cancellation)
                    .ConfigureAwait(false);
            });

        return app;
    }

    private static MeterProviderBuilder AddBuiltInMeters(this MeterProviderBuilder meterProviderBuilder)
    {
        return meterProviderBuilder.AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Server.Kestrel",
            "System.Net.Http");
    }
}
