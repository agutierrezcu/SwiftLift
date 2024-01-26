using Oakton.Environment;
using SimpleInjector;
using SimpleInjector.Diagnostics;

namespace SwiftLift.SharedKernel.SimpleInjector;

internal sealed class ContainerEnvironmentCheck : IEnvironmentCheck
{
    public string Description
        => "Dependencies registration must be valid.";

    public Task Assert(IServiceProvider services, CancellationToken cancellation)
    {
        var container = (Container)services;

        container.Verify(VerificationOption.VerifyAndDiagnose);

        Analyzer.Analyze(container);

        return Task.CompletedTask;
    }
}

