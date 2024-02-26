using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace SwiftLift.IdentityServer.Api.Data;

[EnumExtensions]
internal enum IdentityServerSchema
{
    [Display(Name = "users")]
    Users,

    [Display(Name = "configuration")]
    Configuration,

    [Display(Name = "operational")]
    Operational
}
