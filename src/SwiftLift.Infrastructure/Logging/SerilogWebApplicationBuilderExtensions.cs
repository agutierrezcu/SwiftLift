using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Serilog.Sinks.SystemConsole.Themes;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Options;

using static System.Globalization.CultureInfo;

namespace SwiftLift.Infrastructure.Logging;

public static class SerilogWebApplicationBuilderExtensions
{
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
                var environmentService = serviceProvider.GetRequiredService<IEnvironmentService>();

                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(serviceProvider)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithRequestUserId()
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
                    .WriteToLogStreamFileIf(
                        () => azureFileOptions.Enabled, context.HostingEnvironment, azureFileOptions)
                    .WriteToForDevelopmentIf(
                        () => builder.Environment.IsDevelopment(), environmentService);
            });
    }

    private static TelemetryConfiguration? s_telemetryConfiguration;

    internal static LoggerConfiguration WriteToApplicationInsights(
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

    internal static LoggerConfiguration WriteToLogStreamFileIf(
       this LoggerConfiguration loggerConfiguration,
       Func<bool> requiredCondition,
       IHostEnvironment environment,
       AzureFileLoggingOptions azureFileOptions)
    {
        Guard.Against.Null(requiredCondition);
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(environment);
        Guard.Against.Null(azureFileOptions);

        if (!requiredCondition())
        {
            return loggerConfiguration;
        }

        if (!requiredCondition())
        {
            return loggerConfiguration;
        }

        return loggerConfiguration
            .WriteTo.Async(
                sync => sync.File(
                    path: string.Format(InvariantCulture,
                        azureFileOptions.PathTemplate!, environment.ApplicationName),
                    fileSizeLimitBytes: azureFileOptions.FileSizeLimit,
                    rollOnFileSizeLimit: azureFileOptions.RollOnSizeLimit,
                    retainedFileCountLimit: azureFileOptions.RetainedFileCount,
                    retainedFileTimeLimit: azureFileOptions.RetainTimeLimit,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    formatProvider: InvariantCulture,
                    outputTemplate: TextBasedOutputTemplate
                ));
    }

    internal static LoggerConfiguration WriteToForDevelopmentIf(
       this LoggerConfiguration loggerConfiguration,
       Func<bool> requiredCondition,
       IEnvironmentService environmentService)
    {
        Guard.Against.Null(requiredCondition);
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(environmentService);

        if (!requiredCondition())
        {
            return loggerConfiguration;
        }

        var seqServerUrl = environmentService.GetRequiredVariable("SEQ_SERVER_URL")!;

        return loggerConfiguration
            .WriteTo.Console(
                formatProvider: InvariantCulture,
                outputTemplate: TextBasedOutputTemplate,
                theme: AnsiConsoleTheme.Code)
            .WriteTo.Seq(seqServerUrl);
    }
}
