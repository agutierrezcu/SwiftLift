using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Environment;

var builder = DistributedApplication.CreateBuilder(args);

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

builder.AddProject<Projects.SwiftLift_Riders_Api>("swiftlift.riders.api")
    .WithEnvironment(
        ApplicationInsightSettings.EnvironmentVariable,
        applicationInsightConnectionString.Value);

await builder.Build().RunAsync()
    .ConfigureAwait(false);
