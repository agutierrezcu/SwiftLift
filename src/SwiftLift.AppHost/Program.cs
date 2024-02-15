using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Environment;

var builder = DistributedApplication.CreateBuilder(args);

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

var seqServerUrlEnvironmentVariable = "SEQ_SERVER_URL";

builder
    .AddProject<Projects.SwiftLift_Riders_Api>("swiftlift.riders.api")
    .WithEnvironment(
        ApplicationInsightSettings.EnvironmentVariable,
        applicationInsightConnectionString.Value)
    .WithEnvironment(
        seqServerUrlEnvironmentVariable,
        EnvironmentService.Instance.GetRequiredVariable(seqServerUrlEnvironmentVariable));

await builder.Build().RunAsync()
    .ConfigureAwait(false);
