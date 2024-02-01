using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace SwiftLift.Infrastructure.Checks;

internal class EnvironmentCheckStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            var checkResults =
                EnvironmentChecker.ExecuteAllEnvironmentChecks(app.ApplicationServices)
                    .GetAwaiter().GetResult();

            if (!checkResults.Succeeded())
            {
                var logger = app.ApplicationServices.GetRequiredService<Serilog.ILogger>();

                logger.ForContext<EnvironmentCheckStartupFilter>()
                    .Error("Environment Checks failures {Results}",
                        GetResultsSummary(checkResults));
            }

            checkResults.Assert();

            next(app);
        };
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
}
