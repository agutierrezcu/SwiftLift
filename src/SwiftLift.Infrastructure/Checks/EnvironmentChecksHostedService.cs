using Microsoft.Extensions.Logging;

namespace SwiftLift.Infrastructure.Checks;

internal sealed class EnvironmentChecksHostedService
    (IServiceProvider _serviceProvider, ILogger<EnvironmentChecksHostedService> _logger)
        : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting {nameof(EnvironmentChecksHostedService)} hosted service");

        var checkResults = await EnvironmentChecker.ExecuteAllEnvironmentChecks(
            _serviceProvider, cancellationToken)
                .ConfigureAwait(false);

        if (!checkResults.Succeeded())
        {
            _logger.LogError("Environment Checks failures {Results}",
                GetResultsSummary(checkResults));
        }

        checkResults.Assert();
    }

    private static string GetResultsSummary(EnvironmentCheckResults checkResults)
    {
        Guard.Against.Null(checkResults);

        var sb = new StringWriter();

        sb.WriteLine();
        if (checkResults.Successes.Length > 0)
        {
            sb.WriteLine("=================================================================");
            sb.WriteLine("=                    Successful Checks                          =");
            sb.WriteLine("=================================================================");

            var successes = checkResults.Successes;
            foreach (var text in successes)
            {
                sb.WriteLine("* " + text);
            }

            sb.WriteLine();
        }

        if (checkResults.Failures.Length > 0)
        {
            sb.WriteLine("=================================================================");
            sb.WriteLine("=                          Failures                             =");
            sb.WriteLine("=================================================================");

            var failures = checkResults.Failures;
            foreach (var environmentFailure in failures)
            {
                sb.WriteLine("Failure: " + environmentFailure.Description);
                sb.WriteLine(environmentFailure.Exception.ToString());
                sb.WriteLine();
                sb.WriteLine();
            }
        }

        return sb.ToString();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Stopping {nameof(EnvironmentChecksHostedService)} hosted service");

        return Task.CompletedTask;
    }
}
