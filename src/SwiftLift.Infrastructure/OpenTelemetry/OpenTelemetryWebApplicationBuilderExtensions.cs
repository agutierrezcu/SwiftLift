using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SwiftLift.Infrastructure.Configuration;
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

        builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddBuiltInMeters();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // We want to view all traces in development
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing
                    .AddAspNetCoreInstrumentation()
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

        var otlpExporterEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        var useOtlpExporter = !string.IsNullOrWhiteSpace(otlpExporterEndpoint);

        if (useOtlpExporter)
        {
            builder.Services
                .Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter())
                .ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter())
                .ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());

            if (builder.Environment.IsDevelopment())
            {
                builder.AddExportToSeq(); 
            }
        }

        // Uncomment the following lines to enable the Prometheus exporter (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
        // builder.Services.AddOpenTelemetry()
        //    .WithMetrics(metrics => metrics.AddPrometheusExporter());

        //services.AddOpenTelemetry().UseAzureMonitor();

        return builder;
    }

    private static IHostApplicationBuilder AddExportToSeq(this IHostApplicationBuilder builder)
    {
        Guard.Against.Null(builder);

        var services = builder.Services;

        var seqServerUrl = builder.Configuration.GetRequired("SEQ_SERVER_URL")!;

        services.Configure<OpenTelemetryLoggerOptions>(
            logging =>
                logging.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new($"{seqServerUrl}ingest/otlp/v1/logs");
                    opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                }));

        services.ConfigureOpenTelemetryTracerProvider(
            tracing =>
                tracing
                    .AddSource("SwiftLift.Api.Dummy")
                    .AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = new($"{seqServerUrl}ingest/otlp/v1/traces");
                        opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                    }));

        return builder;
    }
}
