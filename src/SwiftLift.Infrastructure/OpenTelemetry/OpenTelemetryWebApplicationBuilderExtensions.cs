using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.Infrastructure.OpenTelemetry;

public static partial class OpenTelemetryWebApplicationBuilderExtensions
{
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

        builder.AddOpenTelemetryExporters();

        return builder;
    }
    private static MeterProviderBuilder AddBuiltInMeters(this MeterProviderBuilder meterProviderBuilder)
    {
        return meterProviderBuilder.AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Server.Kestrel",
            "System.Net.Http");
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        Guard.Against.Null(builder);

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

        //var resourceAttributes = new Dictionary<string, object> {
        //    { "service.name", applicationInfo.Name },
        //    { "service.namespace", applicationInfo.Namespace },
        //    { "service.instance.id", applicationInfo.Id }
        //};

        //services.ConfigureOpenTelemetryTracerProvider(
        //    config => config.ConfigureResource(
        //        resourceBuilder => resourceBuilder.AddAttributes(resourceAttributes)));

        // TODO: check this out later having into account the issue https://github.com/dotnet/aspire/issues/1562
        //services.AddOpenTelemetry()
        //   .UseAzureMonitor(
        //        opts =>
        //        {
        //            opts.SamplingRatio = 0.05F;

        //            opts.Credential =
        //                builder.Environment.IsDevelopment()
        //                ? new DefaultAzureCredential()
        //                : new ManagedIdentityCredential();
        //        }
        //    );

        return builder;
    }
}
