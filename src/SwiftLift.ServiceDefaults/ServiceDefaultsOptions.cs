using System.Reflection;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.ServiceDefaults;

public sealed class ServiceDefaultsOptions
{
    public required ApplicationInfo ApplicationInfo { get; init; }

    public required Assembly[] ApplicationAssemblies { get; init; }
}
