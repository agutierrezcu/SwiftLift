using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Sensitive;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.ExceptionalLogContext;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Exceptions.Grpc.Destructurers;
using Serilog.Exceptions.Refit.Destructurers;
using Serilog.Sinks.SystemConsole.Themes;
using SwiftLift.Infrastructure.Configuration;
using SwiftLift.Infrastructure.ConnectionString;

namespace SwiftLift.Infrastructure.Logging;

public static class SerilogWebApplicationBuilderExtensions
{
    internal const string TextBasedOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{BuildId} {BuildNumber} {BuildCommit}] [{EventId}] [{EventName}] [{EventType:x8} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static void AddLogging(this WebApplicationBuilder builder,
        ConnectionStringResource applicationInsightConnectionString,
        string azureLogStreamOptionsSectionPath,
        params Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInsightConnectionString);
        Guard.Against.NullOrWhiteSpace(azureLogStreamOptionsSectionPath);
        Guard.Against.NullOrEmpty(applicationAssemblies);

        var services = builder.Services;

        services
            .Scan(scan => scan
                .FromAssemblies(applicationAssemblies)
                .AddClasses(s => s.AssignableTo<ILogEventEnricher>(), false)
                .As<ILogEventEnricher>()
                .WithSingletonLifetime());

        builder.Host.UseSerilog(
            (context, serviceProvider, loggerConfiguration) =>
            {
                var applicationName = context.HostingEnvironment.ApplicationName;

                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(serviceProvider)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.WithProperty("ApplicationName", applicationName)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithSpan(
                        new SpanOptions
                        {
                            IncludeBaggage = false,
                            IncludeOperationName = true,
                            IncludeTags = false,
                            IncludeTraceFlags = false
                        })
                    .Enrich.WithSensitiveDataMasking(opts => opts.Mode = MaskingMode.InArea)
                    .Enrich.WithExceptionalLogContext()
                    .Enrich.WithExceptionDetails(
                        new DestructuringOptionsBuilder()
                            .WithDefaultDestructurers()
                            .WithDestructurers(
                                [
                                    new DbUpdateExceptionDestructurer(),
                                    new RpcExceptionDestructurer(),
                                    new ApiExceptionDestructurer(
                                        destructureCommonExceptionProperties: false,
                                        destructureHttpContent: true
                                    )
                                ])
                            .WithIgnoreStackTraceAndTargetSiteExceptionFilter())
                    .WriteTo.ApplicationInsights(
                        applicationInsightConnectionString.Value,
                        TelemetryConverter.Traces);

                var otlpExporterEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

                var useOtlpExporter = !string.IsNullOrWhiteSpace(otlpExporterEndpoint);

                if (useOtlpExporter)
                {
                    loggerConfiguration
                       .WriteTo.OpenTelemetry(options =>
                       {
                           options.Endpoint = otlpExporterEndpoint!;
                           options.ResourceAttributes.Add("service.name", applicationName); //builder.Configuration["OTEL_SERVICE_NAME"] ?? applicationName);
                       });
                }

                if (context.HostingEnvironment.IsDevelopment())
                {
                    var seqServerUrl = context.Configuration.GetRequired("SEQ_SERVER_URL")!;

                    loggerConfiguration
                        .WriteTo.Console(
                            outputTemplate: TextBasedOutputTemplate,
                            theme: AnsiConsoleTheme.Code)
                        .WriteTo.Seq(seqServerUrl);
                }
            });

        builder.Logging.ClearProviders();
    }

    internal static bool IsAzureLogStreamEnabled(IConfiguration configuration)
    {
        Guard.Against.Null(configuration);

        var azureLogStreamEnabledValue = configuration.GetRequired("AZURE_LOG_STREAM_ENABLED");

        return bool.TryParse(azureLogStreamEnabledValue, out var azureLogStreamEnabled)
                    && azureLogStreamEnabled;
    }
}
