namespace SwiftLift.Infrastructure.Logging;

internal sealed class AzureLogStreamOptionsValidator : AbstractValidator<AzureLogStreamOptions>
{
    public AzureLogStreamOptionsValidator()
    {
        RuleFor(o => o.PathTemplate)
            .NotEmpty()
            .Must(v => v?.Contains("{0}") ?? false)
            .WithMessage("Path template can not be empty and must contains one place holder for Application Name");

        RuleFor(o => o.FileSizeLimit).GreaterThan(0);

        RuleFor(o => o.RetainedFileCount).GreaterThan(0);

        RuleFor(o => o.RetainTimeLimit).GreaterThan(TimeSpan.Zero);
    }
}
