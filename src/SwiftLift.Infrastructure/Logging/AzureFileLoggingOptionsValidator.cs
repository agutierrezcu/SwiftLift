namespace SwiftLift.Infrastructure.Logging;

internal sealed class AzureFileLoggingOptionsValidator : AbstractValidator<AzureFileLoggingOptions>
{
    public AzureFileLoggingOptionsValidator()
    {
        RuleFor(o => o.FileSizeLimit)
            .Must(v => v > 0)
            .WithMessage($"When specified, {nameof(AzureFileLoggingOptions.FileSizeLimit)} must be a positive value");

        RuleFor(x => x.RetainedFileCount)
            .Must(x => x > 0)
            .WithMessage($"When specified, {nameof(AzureFileLoggingOptions.RetainedFileCount)} must be a positive value");

        RuleFor(x => x.RetainTimeLimit)
            .Must(x => x > TimeSpan.Zero)
            .WithMessage($"When specified, {nameof(AzureFileLoggingOptions.RetainTimeLimit)} must specify a non-negative time span value");
    }
}
