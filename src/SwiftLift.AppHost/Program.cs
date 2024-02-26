using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Environment;

var builder = DistributedApplication.CreateBuilder(args);

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

var seqServerUrlEnvironmentVariable = "SEQ_SERVER_URL";
var seqServerUrl = EnvironmentService.Instance
    .GetRequiredVariable(seqServerUrlEnvironmentVariable);

builder
    .AddProject<Projects.SwiftLift_Riders_Api>("swiftlift.riders.api")
    .WithEnvironment(ApplicationInsightSettings.EnvironmentVariable, applicationInsightConnectionString.Value)
    .WithEnvironment(seqServerUrlEnvironmentVariable, seqServerUrl);

var postgrespw = builder.Configuration["postgrespassword"];

if (string.IsNullOrEmpty(postgrespw))
{
    throw new InvalidOperationException(
        @"
        A password for the local SQL Postgres container is not configured.
        Add one to the AppHost project's user secrets with the key 'postgrespassword', e.g. dotnet user-secrets set postgrespassword <password>
        ");
}

var postgresDatabase = builder
    .AddPostgresContainer("postgres", port: 5432, password: postgrespw)
    .WithVolumeMount("VolumeMount.postgres.data", "/var/lib/postgresql/data", VolumeMountType.Named)
    .WithPgAdmin()
    .AddDatabase("identityserverdb");

builder
    .AddProject<Projects.SwiftLift_IdentityServer_Api>("swiftlift.identityserver.api")
    .WithReference(postgresDatabase)
    .WithEnvironment(ApplicationInsightSettings.EnvironmentVariable, applicationInsightConnectionString.Value)
    .WithEnvironment(seqServerUrlEnvironmentVariable, seqServerUrl);

await builder.Build().RunAsync()
    .ConfigureAwait(false);
