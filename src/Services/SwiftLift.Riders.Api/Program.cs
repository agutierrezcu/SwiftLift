using FastEndpoints;
using FastEndpoints.Swagger;
using Serilog;
using SwiftLift.Infrastructure;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Logging;
using SwiftLift.ServiceDefaults;
using SwiftLift.SharedKernel.Application;

var applicationInfo = new ApplicationInfo(
    "swiftlift.riders.api", "Riders.Api", "SwiftLift");

var builder = WebApplication.CreateBuilder(args);

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

Log.Logger = builder.CreateBootstrapLogger(
    applicationInsightConnectionString,
    EnvironmentService.Instance);

Log.Information("Bootstrap logger created.");

try
{
    var applicationAssemblies = AppDomain.CurrentDomain
        .GetApplicationAssemblies(applicationInfo.Namespace);

    builder.AddServiceDefaults(opts =>
    {
        opts.ApplicationInfo = applicationInfo;
        opts.ApplicationInsightConnectionString = applicationInsightConnectionString;
        opts.ApplicationAssemblies = applicationAssemblies;
        opts.AzureLogStreamConfigurationSectionKey = "AzureFileLoggingOptions";
    });

    var services = builder.Services;

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.AddAuthentication();
    services.AddAuthorization();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseHeaderPropagation();

    app.UseSerilogRequestLogging(
        SerilogRequestLoggingOptions.Configure);

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapDefaultEndpoints();

    app.UseFastEndpoints();
    app.UseSwaggerGen();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    await app.RunAppAsync(args, Log.Logger)
        .ConfigureAwait(false);
}
catch (Exception ex)
    when (ex.GetType().Name is not "StopTheHostException" and not "HostAbortedException")
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Shut down complete.");

    await Log.CloseAndFlushAsync()
        .ConfigureAwait(false);
}

