using System.Diagnostics;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Serilog.Sinks.SystemConsole.Themes;
using SwiftLift.Infrastructure.ConnectionString;
using static System.Globalization.CultureInfo;

namespace SwiftLift.Infrastructure.Logging;

[ExcludeFromCodeCoverage]
public static class SerilogWebApplicationBuilderExtensions
{
    public static Serilog.ILogger CreateBootstrapLogger(this WebApplicationBuilder builder,
        string applicationId,
        ConnectionStringResource applicationInsightConnectionString)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrWhiteSpace(applicationId);
        Guard.Against.Null(applicationInsightConnectionString);

        SerilogSelfLogging();

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithProperty("Bootstrapping", true)
            .Enrich.WithProperty("ApplicationId", applicationId)
            .Enrich.WithExceptionDetails(
                new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers())
            .WriteToApplicationInsights(applicationInsightConnectionString)
            .WriteToLogStreamFile(builder.Environment)
            .WriteToConsoleIfDevelopment(builder.Environment)
            .CreateBootstrapLogger();
    }

    public static void AddSerilog(this WebApplicationBuilder builder,
        string applicationId,
        ConnectionStringResource applicationInsightConnectionString)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrWhiteSpace(applicationId);
        Guard.Against.Null(applicationInsightConnectionString);

        builder.Logging
            .ClearProviders()
            // To use Azure Log Stream.
            .AddAzureWebAppDiagnostics();

        builder.Host.UseSerilog(
            (context, serviceProvider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(serviceProvider)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProperty("ApplicationId", applicationId)
                    .Enrich.WithSensitiveDataMasking(opts => opts.Mode = MaskingMode.InArea)
                    .Enrich.WithExceptionDetails(
                        new DestructuringOptionsBuilder()
                            .WithDefaultDestructurers()
                            .WithDestructurers(new[]
                            {
                                new ApiExceptionDestructurer()
                            }))
                    .WriteToApplicationInsights(applicationInsightConnectionString)
                    .WriteToLogStreamFile(context.HostingEnvironment)
                    .WriteToConsoleIfDevelopment(context.HostingEnvironment);
            });
    }

    [Conditional("DEBUG")]
    private static void SerilogSelfLogging()
    {
        SelfLog.Enable(Console.WriteLine);
    }

    private static TelemetryConfiguration? s_telemetryConfiguration;

    private static LoggerConfiguration WriteToApplicationInsights(
       this LoggerConfiguration loggerConfiguration,
       ConnectionStringResource applicationInsightConnectionString)
    {
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(applicationInsightConnectionString);

        s_telemetryConfiguration ??= CreateTelemetryConfiguration(applicationInsightConnectionString);

        return loggerConfiguration.
            WriteTo.ApplicationInsights(
                s_telemetryConfiguration, TelemetryConverter.Traces);
    }

    private static TelemetryConfiguration? CreateTelemetryConfiguration(
        ConnectionStringResource connectionStringResource)
    {
        Guard.Against.Null(connectionStringResource);

        var telemetryConfiguration = TelemetryConfiguration.CreateDefault();

        telemetryConfiguration.ConnectionString = connectionStringResource.Value;
        telemetryConfiguration.DisableTelemetry = true;

        return telemetryConfiguration;
    }

    private const string TextBasedOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{EventId}] [{EventName}] [{EventType:x8} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    private static LoggerConfiguration WriteToLogStreamFile(
       this LoggerConfiguration loggerConfiguration,
       IHostEnvironment environment)
    {
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(environment);

        var rootLocation = environment.IsDevelopment() ? "C" : "D";

        var options = new LoggingConfigOptions();

        return loggerConfiguration
            .WriteTo.Async(
                x => x.File(
                    $@"{rootLocation}:\home\LogFiles\Application\{environment.ApplicationName}.txt",
                    fileSizeLimitBytes: options.AzureFileSizeLimit,
                    rollOnFileSizeLimit: options.AzureRollOnSizeLimit,
                    retainedFileCountLimit: options.AzureRetainedFileCount,
                    retainedFileTimeLimit: options.AzureRetainTimeLimit,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    formatProvider: InvariantCulture,
                    outputTemplate: TextBasedOutputTemplate
                ));
    }

    private static LoggerConfiguration WriteToConsoleIfDevelopment(
       this LoggerConfiguration loggerConfiguration,
       IHostEnvironment environment)
    {
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(environment);

        if (!environment.IsDevelopment())
        {
            return loggerConfiguration;
        }

        return loggerConfiguration
            .WriteTo.Console(
                formatProvider: InvariantCulture,
                outputTemplate: TextBasedOutputTemplate,
                theme: AnsiConsoleTheme.Code
            );
    }
}
