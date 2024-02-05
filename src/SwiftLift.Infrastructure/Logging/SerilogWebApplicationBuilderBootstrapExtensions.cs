using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Sinks.SystemConsole.Themes;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Logging;

using static SwiftLift.Infrastructure.Logging.SerilogWebApplicationBuilderExtensions;

namespace SwiftLift.Infrastructure.Logging;

public static class SerilogWebApplicationBuilderBootstrapExtensions
{
    public static ILogger CreateBootstrapLogger(this WebApplicationBuilder builder,
        ConnectionStringResource applicationInsightConnectionString,
        IEnvironmentService environmentService)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInsightConnectionString);
        Guard.Against.Null(environmentService);

        var loggerConfiguration =
            new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Bootstrapping", true)
                .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
                .Enrich.WithExceptionDetails(
                    new DestructuringOptionsBuilder()
                        .WithDefaultDestructurers())
                .WriteTo.ApplicationInsights(
                    applicationInsightConnectionString.Value,
                    TelemetryConverter.Traces);

        var azureLogStreamEnabledValue = environmentService.GetVariable("AZURE_LOG_STREAM_ENABLED");

        if (bool.TryParse(azureLogStreamEnabledValue, out var azureLogStreamEnabled) && azureLogStreamEnabled)
        {
            var azureLogStreamOptions = AzureLogStreamOptions.CreateDefault(builder.Environment);
            var azureFileLoggingOptionsValidator = new AzureLogStreamOptionsValidator();
            azureFileLoggingOptionsValidator.ValidateAndThrow(azureLogStreamOptions);

            loggerConfiguration
                .WriteToLogStreamFile(
                    azureLogStreamOptions,
                    builder.Environment.ApplicationName);
        }

        if (builder.Environment.IsDevelopment())
        {
            var seqServerUrl = environmentService.GetRequiredVariable("SEQ_SERVER_URL")!;

            loggerConfiguration
                .WriteTo.Console(
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: TextBasedOutputTemplate,
                    theme: AnsiConsoleTheme.Code)
                .WriteTo.Seq(seqServerUrl);
        }

        return loggerConfiguration
            .CreateBootstrapLogger();
    }
}
