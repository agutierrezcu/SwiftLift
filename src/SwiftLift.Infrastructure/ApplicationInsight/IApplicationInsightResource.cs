using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;

namespace SwiftLift.Infrastructure.ApplicationInsight;

public interface IApplicationInsightResource
{
    ConnectionStringResource GetConnectionStringGuaranteed(
        IEnvironmentService environmentService, IConfiguration configuration);
}
