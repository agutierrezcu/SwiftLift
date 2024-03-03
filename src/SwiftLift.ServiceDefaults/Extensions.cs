using System.Net.Mime;
using Ardalis.GuardClauses;
using FastEndpoints;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using SwiftLift.Infrastructure.Activity;
using SwiftLift.Infrastructure.BuildInfo;
using SwiftLift.Infrastructure.Checks;
using SwiftLift.Infrastructure.Correlation;
using SwiftLift.Infrastructure.FastEndpoints;
using SwiftLift.Infrastructure.HealthChecks;
using SwiftLift.Infrastructure.HttpClient;
using SwiftLift.Infrastructure.Logging;
using SwiftLift.Infrastructure.OpenTelemetry;
using SwiftLift.Infrastructure.Operations;
using SwiftLift.Infrastructure.Serialization;
using SwiftLift.Infrastructure.UserContext;
using SwiftLift.Infrastructure.Validators;

namespace SwiftLift.ServiceDefaults;

public static partial class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder,
        ServiceDefaultsOptions serviceDefaultsOptions)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(serviceDefaultsOptions);

        builder.Host.UseDefaultServiceProvider(opts =>
        {
            opts.ValidateScopes = true;
            opts.ValidateOnBuild = true;
        });

        var serviceDefaultsOptionsValidator = new ServiceDefaultsOptionsValidator();
        serviceDefaultsOptionsValidator.ValidateAndThrow(serviceDefaultsOptions);

        var applicationInfo = serviceDefaultsOptions.ApplicationInfo;
        var applicationAssemblies = serviceDefaultsOptions.ApplicationAssemblies;

        builder.AddLogging(applicationAssemblies);

        if (serviceDefaultsOptions.UseFastEndpoints)
        {
            builder.AddFastEndpoints(applicationAssemblies);
        }

        builder.ConfigureOpenTelemetry(applicationAssemblies);

        builder.AddEnvironmentChecks(applicationAssemblies);

        builder.AddHealthChecks(applicationAssemblies);

        var services = builder.Services;

        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddFeatureManagement();
        services.AddProblemDetails();

        services.AddBuildInfo();

        services.AddCorrelationId();

        services.AddUserContext();

        services.ConfigureHttpClient();

        services.AddValidators(applicationAssemblies);

        services.AddActivitySourceProvider();

        services.AddSnakeSerialization();

        services.AddSingleton(_ => applicationInfo);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Uncomment the following line to enable the Prometheus endpoint (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
        // app.MapPrometheusScrapingEndpoint();

        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter =
                    app.Environment.IsDevelopment()
                    ? UIResponseWriter.WriteHealthCheckUIResponse
                    : (_, _) => Task.CompletedTask
            });

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        app.MapGet("/build-info",
            async (IBuildProvider buildProvider, HttpResponse response, CancellationToken cancellation) =>
            {
                var fileContent =
                    await buildProvider.GetBuildAsJsonAsync(cancellation)
                        .ConfigureAwait(false);

                response.ContentType = MediaTypeNames.Application.Json;

                await response.WriteAsync(fileContent, cancellation)
                    .ConfigureAwait(false);
            })
            .WithDisplayName(OperationEndpoint.BuildInfo.ToStringFast())
            .ExcludeFromDescription();

        return app;
    }
}
