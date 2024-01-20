using Microsoft.Extensions.Configuration;
using SwiftLift.SharedKernel.ConnectionString;
using SwiftLift.SharedKernel.Environment;

namespace SwiftLift.SharedKernel.ApplicationInsight;

public interface IApplicationInsightResource
{
    ConnectionStringResource GetConnectionStringGuaranteed(
        IEnvironmentService environmentService, IConfiguration configuration);
}
