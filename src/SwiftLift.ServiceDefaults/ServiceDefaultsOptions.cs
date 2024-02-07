using System.Reflection;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.ServiceDefaults;

public sealed class ServiceDefaultsOptions
{
    public required ApplicationInfo ApplicationInfo { get; set; }

    public required ConnectionStringResource ApplicationInsightConnectionString { get; set; }

    public required Assembly[] ApplicationAssemblies { get; set; }

    public string AzureLogStreamOptionsSectionPath { get; set; } = "AzureLogStream";

    public IEnvironmentService EnvironmentService { get; set; } = Infrastructure.Environment.EnvironmentService.Instance;
}
