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
using SwiftLift.Infrastructure.Options;

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
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.WithProperty("Bootstrapping", true)
            .Enrich.WithProperty("ApplicationId", applicationId)
            .Enrich.WithExceptionDetails(
                new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers())
            .WriteToApplicationInsights(applicationInsightConnectionString)
            .WriteToLogStreamFile(builder.Environment, new AzureFileLoggingOptions())
            .WriteToConsoleIfDevelopment(builder.Environment)
            .CreateBootstrapLogger();
    }

    public static void AddLogging(this WebApplicationBuilder builder,
        string applicationId,
        ConnectionStringResource applicationInsightConnectionString,
        string azureFileLoggingOptionsConfigurationKey)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrWhiteSpace(applicationId);
        Guard.Against.Null(applicationInsightConnectionString);
        Guard.Against.NullOrWhiteSpace(azureFileLoggingOptionsConfigurationKey);

        builder.Logging
            .ClearProviders();

        builder.Services
            .ConfigureOptions<AzureFileLoggingOptions, AzureFileLoggingOptionsValidator>(
                azureFileLoggingOptionsConfigurationKey,
                opts => opts.RegisterAsSingleton = true);

        builder.Host.UseSerilog(
            (context, serviceProvider, loggerConfiguration) =>
            {
                var azureFileOptions = serviceProvider.GetRequiredService<AzureFileLoggingOptions>();

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
                    .WriteToLogStreamFile(context.HostingEnvironment, azureFileOptions)
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
       IHostEnvironment environment,
       AzureFileLoggingOptions loggingConfigOptions)
    {
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(environment);
        Guard.Against.Null(loggingConfigOptions);

        if (!loggingConfigOptions.Enabled)
        {
            return loggerConfiguration;
        }

        var rootLocation = environment.IsDevelopment() ? "C" : "D";

        return loggerConfiguration
            .WriteTo.Async(
                x => x.File(
                    $@"{rootLocation}:\home\LogFiles\Application\{environment.ApplicationName}.txt",
                    fileSizeLimitBytes: loggingConfigOptions.FileSizeLimit,
                    rollOnFileSizeLimit: loggingConfigOptions.RollOnSizeLimit,
                    retainedFileCountLimit: loggingConfigOptions.RetainedFileCount,
                    retainedFileTimeLimit: loggingConfigOptions.RetainTimeLimit,
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
