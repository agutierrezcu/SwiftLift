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
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Options;

namespace SwiftLift.Infrastructure.Logging;

public static class SerilogWebApplicationBuilderExtensions
{
    public static void AddLogging(this WebApplicationBuilder builder,
        ConnectionStringResource applicationInsightConnectionString,
        IEnvironmentService environmentService,
        string azureLogStreamOptionsSectionPath,
        params Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInsightConnectionString);
        Guard.Against.Null(environmentService);
        Guard.Against.NullOrWhiteSpace(azureLogStreamOptionsSectionPath);
        Guard.Against.NullOrEmpty(applicationAssemblies);

        var services = builder.Services;

        var isAzureLogStreamEnabled = IsAzureLogStreamEnabled(environmentService);

        if (isAzureLogStreamEnabled)
        {
            services
                .ConfigureOptions<AzureLogStreamOptions, AzureLogStreamOptionsValidator>(
                    azureLogStreamOptionsSectionPath,
                    opts => opts.RegisterAsSingleton = true);
        }

        services
            .Scan(scan => scan
                .FromAssemblies(applicationAssemblies)
                .AddClasses(s => s.AssignableTo<ILogEventEnricher>(), false)
                .As<ILogEventEnricher>()
                .WithSingletonLifetime());

        builder.Logging
          .ClearProviders();

        builder.Host.UseSerilog(
            (context, serviceProvider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(serviceProvider)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithSpan()
                    .Enrich.WithExceptionalLogContext()
                    .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
                    .Enrich.WithSensitiveDataMasking(opts => opts.Mode = MaskingMode.InArea)
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

                if (isAzureLogStreamEnabled)
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
                            outputTemplate: TextBasedOutputTemplate,
                            theme: AnsiConsoleTheme.Code)
                        .WriteTo.Seq(seqServerUrl);
                }
            });
    }

    internal static bool IsAzureLogStreamEnabled(IEnvironmentService environmentService)
    {
        var azureLogStreamEnabledValue = environmentService.GetVariable("AZURE_LOG_STREAM_ENABLED");

        return bool.TryParse(azureLogStreamEnabledValue, out var azureLogStreamEnabled)
                    && azureLogStreamEnabled;
    }

    internal const string TextBasedOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{BuildId} {BuildNumber} {BuildCommit}] [{EventId}] [{EventName}] [{EventType:x8} {Level:u3}] {Message:lj}{NewLine}{Exception}";

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
                    path: string.Format(azureFileOptions.PathTemplate!, applicationName),
                    fileSizeLimitBytes: azureFileOptions.FileSizeLimit,
                    rollOnFileSizeLimit: azureFileOptions.RollOnSizeLimit,
                    retainedFileCountLimit: azureFileOptions.RetainedFileCount,
                    retainedFileTimeLimit: azureFileOptions.RetainTimeLimit,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    outputTemplate: TextBasedOutputTemplate
                ));
    }
}
