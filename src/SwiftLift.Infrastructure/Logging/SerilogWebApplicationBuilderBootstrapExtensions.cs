using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Logging;

namespace SwiftLift.Infrastructure.Logging;

public static class SerilogWebApplicationBuilderBootstrapExtensions
{
    public static ILogger CreateBootstrapLogger(this WebApplicationBuilder builder,
        string applicationId,
        ConnectionStringResource applicationInsightConnectionString)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrWhiteSpace(applicationId);
        Guard.Against.Null(applicationInsightConnectionString);

        var azureFileOptions = AzureFileLoggingOptions.CreateDefault(builder.Environment);

        var azureFileLoggingOptionsValidator = new AzureFileLoggingOptionsValidator();
        azureFileLoggingOptionsValidator.ValidateAndThrow(azureFileOptions);

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Bootstrapping", true)
            .Enrich.WithProperty("ApplicationId", applicationId)
            .Enrich.WithExceptionDetails(
                new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers())
            .WriteToApplicationInsights(applicationInsightConnectionString)
            .WriteToLogStreamFileIf(
                () => azureFileOptions.Enabled, builder.Environment, azureFileOptions)
            .WriteToForDevelopmentIf(
                () => builder.Environment.IsDevelopment(), EnvironmentService.Instance)
            .CreateBootstrapLogger();
    }
}
