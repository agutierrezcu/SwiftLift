using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace SwiftLift.Infrastructure.Operations;

[EnumExtensions]
public enum OperationEndpoint
{
    [Display(Name = "Health checks")]
    HealthChecks,

    [Display(Name = "Build info")]
    BuildInfo
}

