using FastEndpoints;
using FastEndpoints.Swagger;
using Serilog;
using SwiftLift.Infrastructure;
using SwiftLift.Infrastructure.FastEndpoints;
using SwiftLift.Infrastructure.Logging;
using SwiftLift.ServiceDefaults;
using SwiftLift.SharedKernel.Application;

var applicationInfo = new ApplicationInfo(
    "swiftlift.riders.api", "Riders.Api", "SwiftLift");

var builder = WebApplication.CreateBuilder(args);

Log.Logger = builder.CreateBootstrapLogger();

Log.Information("Starting {ApplicationId} service up", applicationInfo.Id);

try
{
    var applicationAssemblies = AppDomain.CurrentDomain
        .GetApplicationAssemblies(applicationInfo.Namespace);

    builder.AddFastEndpoints(applicationAssemblies);

    ServiceDefaultsOptions serviceDefaultsOptions = new()
    {
        ApplicationInfo = applicationInfo,
        ApplicationAssemblies = applicationAssemblies
    };

    builder.AddServiceDefaults(serviceDefaultsOptions);

    var services = builder.Services;

    services.AddTracingSourceNames();

    services.AddAuthentication();
    services.AddAuthorization();

    var app = builder.Build();

    app.UseSerilogRequestLogging(
           SerilogRequestLoggingOptions.Configure);

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseHeaderPropagation();

    app.UseFastEndpoints();

    app.MapDefaultEndpoints();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerGen();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    await app.RunAppAsync(args)
        .ConfigureAwait(false);
}
catch (Exception ex)
    when (ex.GetType().Name is not "StopTheHostException" and not "HostAbortedException")
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Shut down complete");

    await Log.CloseAndFlushAsync()
        .ConfigureAwait(false);
}

