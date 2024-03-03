using System.Net.Sockets;
using SwiftLift.Infrastructure.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var seqServerUrlConfigKey = "SEQ_SERVER_URL";
var seqServerUrl = configuration.GetRequiredValue(seqServerUrlConfigKey);

builder
    .AddResource(new ContainerResource("seq"))
    .WithAnnotation(new EndpointAnnotation(ProtocolType.Tcp, uriScheme: "http", name: "seq", port: 5341, containerPort: 80))
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithAnnotation(new ContainerImageAnnotation { Image = "datalust/seq", Tag = "latest" });

builder
    .AddProject<Projects.SwiftLift_Riders_Api>("swiftlift.riders.api")
    .WithEnvironment(seqServerUrlConfigKey, seqServerUrl);

var postgrespw = configuration.GetRequiredValue("postgrespassword");

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
    .WithEnvironment(seqServerUrlConfigKey, seqServerUrl);

await builder.Build().RunAsync()
    .ConfigureAwait(false);
