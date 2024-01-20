using SwiftLift.SharedKernel.ApplicationInsight;
using SwiftLift.SharedKernel.Environment;

var builder = DistributedApplication.CreateBuilder(args);

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

builder.AddProject<Projects.SwiftLift_Riders_Api>("swiftlift.riders.api")
    .WithEnvironment(
        ApplicationInsightResourceDefaults.EnvironmentVariable,
        applicationInsightConnectionString.Value);

await builder.Build().RunAsync()
    .ConfigureAwait(false);
