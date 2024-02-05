using System.Reflection;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.ServiceDefaults;

public sealed class ServiceDefaultsOptions
{
    public ApplicationInfo ApplicationInfo { get; set; }

    public ConnectionStringResource ApplicationInsightConnectionString { get; set; }

    public Assembly[] ApplicationAssemblies { get; set; }

    public string AzureLogStreamConfigurationSectionKey { get; set; }
}
