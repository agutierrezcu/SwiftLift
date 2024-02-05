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

        RuleFor(o => o.ApplicationAssemblies).NotEmpty()
            .WithMessage("ApplicationAssemblies must not be null or empty.");

        RuleFor(o => o.AzureLogStreamConfigurationSectionKey).NotEmpty()
            .WithMessage("AzureLogStreamConfigurationSectionKey must not be empty.");
    }

    private sealed class ApplicationInfoValidator : AbstractValidator<ApplicationInfo>
    {
        public ApplicationInfoValidator()
        {
            RuleFor(ai => ai.Id).NotEmpty()
                .WithMessage("Application Id must not be empty.");

            RuleFor(ai => ai.Name).NotEmpty()
                .WithMessage("Application Name must not be empty.");

            RuleFor(ai => ai.Namespace).NotEmpty()
                .WithMessage("Application Namespace must not be empty.");
        }
    }

    private sealed class ConnectionStringResourceValidator : AbstractValidator<ConnectionStringResource>
    {
        public ConnectionStringResourceValidator()
        {
            RuleFor(cs => cs.Name).NotEmpty()
                .WithMessage("Name must not be empty.");

            RuleFor(cs => cs.Value).NotEmpty()
                .WithMessage("Value must not be empty.");
        }
    }
}
