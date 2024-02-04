namespace SwiftLift.Infrastructure.Logging;

internal sealed class AzureFileLoggingOptionsValidator : AbstractValidator<AzureFileLoggingOptions>
{
    public AzureFileLoggingOptionsValidator()
    {
        RuleFor(o => o.PathTemplate)
            .NotEmpty()
            .Must(v => v?.Contains("{0}") ?? false)
            .WithMessage("Path template can not be empty and must contains one place holder");

        RuleFor(o => o.FileSizeLimit)
            .Must(v => v > 0)
            .WithMessage($"When specified, {nameof(AzureFileLoggingOptions.FileSizeLimit)} must be a positive value");

        RuleFor(o => o.RetainedFileCount)
            .Must(v => v > 0)
            .WithMessage($"When specified, {nameof(AzureFileLoggingOptions.RetainedFileCount)} must be a positive value");

        RuleFor(o => o.RetainTimeLimit)
            .Must(v => v > TimeSpan.Zero)
            .WithMessage($"When specified, {nameof(AzureFileLoggingOptions.RetainTimeLimit)} must specify a non-negative time span value");
    }
}
