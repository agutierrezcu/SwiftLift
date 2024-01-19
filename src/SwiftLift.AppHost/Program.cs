var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.SwiftLift_ApiService>("apiservice");

builder.AddProject<Projects.SwiftLift_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
