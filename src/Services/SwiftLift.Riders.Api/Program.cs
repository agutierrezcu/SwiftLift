using Serilog;
using SwiftLift.Infrastructure;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Logging;
using SwiftLift.ServiceDefaults;
using SwiftLift.SharedKernel.Application;

var builder = WebApplication.CreateBuilder(args);

var applicationInfo = new ApplicationInfo("swiftlift.riders.api", "Riders.Api", "SwiftLift");

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

Log.Logger = builder.CreateBootstrapLogger(applicationInfo.Id,
    applicationInsightConnectionString);

Log.Logger.Information("Logger created.");

try
{
    var assemblies = AppDomain.CurrentDomain
        .GetApplicationAssemblies(applicationInfo.Namespace);

    builder.AddServiceDefaults(
        applicationInfo,
        applicationInsightConnectionString,
        assemblies);

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

    app.UseHttpsRedirection();

    app.UseHeaderPropagation();

    app.UseSerilogRequestLogging();

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapDefaultEndpoints();

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

    Log.CloseAndFlush();
}

