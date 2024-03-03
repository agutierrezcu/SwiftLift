using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Sinks.SystemConsole.Themes;
using SwiftLift.Infrastructure.Configuration;
using SwiftLift.Infrastructure.Logging;

using static SwiftLift.Infrastructure.Logging.SerilogWebApplicationBuilderExtensions;

namespace SwiftLift.Infrastructure.Logging;

public static class SerilogWebApplicationBuilderBootstrapExtensions
{
    public static ILogger CreateBootstrapLogger(this WebApplicationBuilder builder)
    {
        Guard.Against.Null(builder);

        var loggerConfiguration =
            new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithSpan()
                .Enrich.WithProperty("Bootstrapping", true)
                .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
                .Enrich.WithExceptionDetails(
                    new DestructuringOptionsBuilder()
                        .WithDefaultDestructurers());

        if (builder.Environment.IsDevelopment())
        {
            var seqServerUrl = builder.Configuration.GetRequiredValue("SEQ_SERVER_URL")!;

            loggerConfiguration
                .WriteTo.Console(
                    outputTemplate: TextBasedOutputTemplate,
                    theme: AnsiConsoleTheme.Code)
                .WriteTo.Seq(seqServerUrl);
        }

        return loggerConfiguration
            .CreateBootstrapLogger();
    }
}
