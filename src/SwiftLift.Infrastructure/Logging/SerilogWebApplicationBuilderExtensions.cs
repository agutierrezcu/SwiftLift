using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Sensitive;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Serilog.Sinks.SystemConsole.Themes;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Options;

namespace SwiftLift.Infrastructure.Logging;

public static class SerilogWebApplicationBuilderExtensions
{
    public static void AddLogging(this WebApplicationBuilder builder,
        ConnectionStringResource applicationInsightConnectionString,
        string azureFileLoggingOptionsConfigurationKey,
        Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInsightConnectionString);
        Guard.Against.NullOrWhiteSpace(azureFileLoggingOptionsConfigurationKey);
        Guard.Against.NullOrEmpty(applicationAssemblies);

        var services = builder.Services;

        services
            .ConfigureOptions<AzureLogStreamOptions, AzureLogStreamOptionsValidator>(
                azureFileLoggingOptionsConfigurationKey,
                opts => opts.RegisterAsSingleton = true);

        services
            .Scan(scan => scan
                .FromAssemblies(applicationAssemblies)
                .AddClasses(s => s.AssignableTo<ILogEventEnricher>(), false)
                .As<ILogEventEnricher>()
                .WithSingletonLifetime()
            );

        builder.Logging
          .ClearProviders();

        builder.Host.UseSerilog(
            (context, serviceProvider, loggerConfiguration) =>
            {
                var environmentService = serviceProvider.GetRequiredService<IEnvironmentService>();

                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(serviceProvider)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithSpan()
                    .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
                    .Enrich.WithSensitiveDataMasking(opts => opts.Mode = MaskingMode.InArea)
                    .Enrich.WithExceptionDetails(
                        new DestructuringOptionsBuilder()
                            .WithDefaultDestructurers()
                            .WithDestructurers(new[]
                            {
                                new ApiExceptionDestructurer()
                            }))
                    .WriteTo.ApplicationInsights(
                        applicationInsightConnectionString.Value,
                        TelemetryConverter.Traces);

                var azureLogStreamEnabledValue = environmentService.GetVariable("AZURE_LOG_STREAM_ENABLED");

                if (bool.TryParse(azureLogStreamEnabledValue, out var azureLogStreamEnabled) && azureLogStreamEnabled)
                {
                    var azureLogStreamOptions = serviceProvider.GetRequiredService<AzureLogStreamOptions>();

                    loggerConfiguration
                        .WriteToLogStreamFile(
                            azureLogStreamOptions,
                            context.HostingEnvironment.ApplicationName);
                }

                if (context.HostingEnvironment.IsDevelopment())
                {
                    var seqServerUrl = environmentService.GetRequiredVariable("SEQ_SERVER_URL")!;

                    loggerConfiguration
                        .WriteTo.Console(
                            formatProvider: CultureInfo.InvariantCulture,
                            outputTemplate: TextBasedOutputTemplate,
                            theme: AnsiConsoleTheme.Code)
                        .WriteTo.Seq(seqServerUrl);
                }
            });
    }

    internal const string TextBasedOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{EventId}] [{EventName}] [{EventType:x8} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    internal static LoggerConfiguration WriteToLogStreamFile(
       this LoggerConfiguration loggerConfiguration,
       AzureLogStreamOptions azureFileOptions,
       string applicationName)
    {
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(azureFileOptions);
        Guard.Against.NullOrWhiteSpace(applicationName);

        return loggerConfiguration
            .WriteTo.Async(
                sync => sync.File(
                    path: string.Format(CultureInfo.InvariantCulture,
                        azureFileOptions.PathTemplate!, applicationName),
                    fileSizeLimitBytes: azureFileOptions.FileSizeLimit,
                    rollOnFileSizeLimit: azureFileOptions.RollOnSizeLimit,
                    retainedFileCountLimit: azureFileOptions.RetainedFileCount,
                    retainedFileTimeLimit: azureFileOptions.RetainTimeLimit,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: TextBasedOutputTemplate
                ));
    }
}
