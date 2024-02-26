using System.Reflection;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.ServiceDefaults;

public sealed class ServiceDefaultsOptions
{
    public required ApplicationInfo ApplicationInfo { get; init; }

    public bool UseFastEndpoints { get; init; } = true;

    public required ConnectionStringResource ApplicationInsightConnectionString { get; init; }

    public required Assembly[] ApplicationAssemblies { get; init; }

    public string AzureLogStreamOptionsSectionPath { get; init; } = "AzureLogStream";

    public IEnvironmentService EnvironmentService { get; init; } =
        Infrastructure.Environment.EnvironmentService.Instance;
}
