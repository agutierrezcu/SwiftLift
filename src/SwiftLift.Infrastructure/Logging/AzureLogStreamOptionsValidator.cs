namespace SwiftLift.Infrastructure.Logging;

internal sealed class AzureLogStreamOptionsValidator : AbstractValidator<AzureLogStreamOptions>
{
    public AzureLogStreamOptionsValidator()
    {
        RuleFor(o => o.PathTemplate)
            .NotEmpty()
            .Must(v => v?.Contains("{0}") ?? false)
            .WithMessage("Path template can not be empty and must contains one place holder");

        RuleFor(o => o.FileSizeLimit)
            .Must(v => v > 0)
            .WithMessage($"When specified, {nameof(AzureLogStreamOptions.FileSizeLimit)} must be a positive value");

        RuleFor(o => o.RetainedFileCount)
            .Must(v => v > 0)
            .WithMessage($"When specified, {nameof(AzureLogStreamOptions.RetainedFileCount)} must be a positive value");

        RuleFor(o => o.RetainTimeLimit)
            .Must(v => v > TimeSpan.Zero)
            .WithMessage($"When specified, {nameof(AzureLogStreamOptions.RetainTimeLimit)} must specify a non-negative time span value");
    }
}
