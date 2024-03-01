using SwiftLift.Infrastructure.ConnectionString;

namespace SwiftLift.Infrastructure.ApplicationInsight;

public interface IApplicationInsightResource
{
    ConnectionStringResource GetConnectionStringGuaranteed(IConfiguration configuration);
}
