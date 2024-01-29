using System.Diagnostics;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Debugging;
using Serilog.Enrichers.Sensitive;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using SwiftLift.Infrastructure.ConnectionString;
using static System.Globalization.CultureInfo;

namespace SwiftLift.Infrastructure.Logging;

[ExcludeFromCodeCoverage]
public static class SerilogWebApplicationBuilderExtensions
{
    [Conditional("DEBUG")]
    private static void SerilogSelfLogging()
    {
        SelfLog.Enable(Console.WriteLine);
    }

    public static ILogger CreateBootstrapLogger(this WebApplicationBuilder builder,
        string applicationId,
        ConnectionStringResource applicationInsightConnectionString)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrWhiteSpace(applicationId);
        Guard.Against.Null(applicationInsightConnectionString);

        SerilogSelfLogging();

        var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
        telemetryConfiguration.ConnectionString = applicationInsightConnectionString.Value;
        telemetryConfiguration.DisableTelemetry = true;

        var loggerConfiguration =
            new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithProperty("Bootstrapping", true)
                .Enrich.WithProperty("ApplicationId", applicationId)
                .Enrich.WithExceptionDetails(
                    new DestructuringOptionsBuilder()
                        .WithDefaultDestructurers())
                .WriteTo.ApplicationInsights(
                    telemetryConfiguration, TelemetryConverter.Traces);

        var rootLocation = builder.Environment.IsDevelopment() ? "C" : "D";

        var options = new LoggingConfigOptions();

        return loggerConfiguration
            .WriteTo.Async(
                x => x.File(
                    $@"{rootLocation}:\home\LogFiles\Application\{builder.Environment.ApplicationName}.txt",
                    fileSizeLimitBytes: options.AzureFileSizeLimit,
                    rollOnFileSizeLimit: options.AzureRollOnSizeLimit,
                    retainedFileCountLimit: options.AzureRetainedFileCount,
                    retainedFileTimeLimit: options.AzureRetainTimeLimit,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    formatProvider: InvariantCulture
                ))
            .CreateBootstrapLogger();
    }

    public static void UseSerilogLogger(this WebApplicationBuilder builder)
    {
        Guard.Against.Null(builder);

        var rootLocation = builder.Environment.IsDevelopment() ? "C" : "D";

        var options = new LoggingConfigOptions();

        builder.Host.UseSerilog(
            (context, serviceProvider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(serviceProvider)
                    .Enrich.WithCorrelationIdHeader("x-correlation-id") // TODO: Move to HeaderKeyNames class
                    .Enrich.WithSensitiveDataMasking(x => x.Mode = MaskingMode.InArea)
                    .Enrich.WithExceptionDetails(
                        new DestructuringOptionsBuilder()
                            .WithDefaultDestructurers()
                            .WithDestructurers(new[]
                            {
                                new ApiExceptionDestructurer()
                            }))
                    .WriteTo.Async(sink =>
                        sink.File(
                            $@"{rootLocation}:\home\LogFiles\Application\{builder.Environment.ApplicationName}.txt",
                            fileSizeLimitBytes: options.AzureFileSizeLimit,
                            rollOnFileSizeLimit: options.AzureRollOnSizeLimit,
                            retainedFileCountLimit: options.AzureRetainedFileCount,
                            retainedFileTimeLimit: options.AzureRetainTimeLimit,
                            shared: true,
                            flushToDiskInterval: TimeSpan.FromSeconds(1),
                            formatProvider: InvariantCulture
                ));
            });
    }
}
