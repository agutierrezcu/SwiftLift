using FluentValidation;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.ServiceDefaults;

public class ServiceDefaultsOptionsValidator : AbstractValidator<ServiceDefaultsOptions>
{
    public ServiceDefaultsOptionsValidator()
    {
        RuleFor(o => o.ApplicationInfo)
            .SetValidator(new ApplicationInfoValidator());

        RuleFor(o => o.ApplicationInsightConnectionString)
            .SetValidator(new ConnectionStringResourceValidator());

        RuleFor(o => o.ApplicationAssemblies).NotEmpty();

        RuleFor(o => o.AzureLogStreamOptionsSectionPath).NotEmpty();
    }

    private sealed class ApplicationInfoValidator : AbstractValidator<ApplicationInfo>
    {
        public ApplicationInfoValidator()
        {
            RuleFor(ai => ai).NotNull();

            When(ai => ai is not null, () =>
            {
                RuleFor(ai => ai.Id).NotEmpty();

                RuleFor(ai => ai.Name).NotEmpty();

                RuleFor(ai => ai.Namespace).NotEmpty();
            });
        }
    }

    private sealed class ConnectionStringResourceValidator : AbstractValidator<ConnectionStringResource>
    {
        public ConnectionStringResourceValidator()
        {
            RuleFor(cs => cs).NotNull();

            When(cs => cs is not null, () =>
            {
                RuleFor(cs => cs.Name).NotEmpty();

                RuleFor(cs => cs.Value).NotEmpty();
            });
        }
    }
}
