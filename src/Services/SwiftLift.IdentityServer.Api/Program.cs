using Serilog;
using SwiftLift.IdentityServer.Api;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Logging;
using SwiftLift.ServiceDefaults;
using SwiftLift.SharedKernel.Application;

var applicationInfo = new ApplicationInfo(
    "swiftlift.identityserver.api", "IdentityServer.Api", "SwiftLift");

var builder = WebApplication.CreateBuilder(args);

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

Log.Logger = builder.CreateBootstrapLogger(
    applicationInsightConnectionString,
    EnvironmentService.Instance);

Log.Information("Starting {ApplicationId} service up", applicationInfo.Id);

try
{
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices(applicationInfo, applicationInsightConnectionString)
        .ConfigurePipeline();

    // this seeding is only for the template to bootstrap the DB and users.
    // in production you will likely want a different approach.
    if (args.Contains("/seed"))
    {
        Log.Information("Seeding database...");
        SeedData.EnsureSeedData(app);
        Log.Information("Done seeding database. Exiting.");
        return;
    }

    await app.RunAppAsync(args)
        .ConfigureAwait(false);
}
catch (Exception ex)
    when (ex.GetType().Name is not "StopTheHostException" and not "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");

    Log.CloseAndFlush();
}
