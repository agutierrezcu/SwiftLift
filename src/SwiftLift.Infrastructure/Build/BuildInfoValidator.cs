namespace SwiftLift.Infrastructure.Build;

internal sealed class BuildInfoValidator : AbstractValidator<BuildInfo>
{
    public BuildInfoValidator()
    {
        RuleFor(buildInfo => buildInfo.Id).NotEmpty();
        RuleFor(buildInfo => buildInfo.Number).NotEmpty();
        RuleFor(buildInfo => buildInfo.Branch).NotEmpty();
        RuleFor(buildInfo => buildInfo.Commit).NotEmpty();

        RuleFor(buildInfo => buildInfo.Url).NotEmpty()
            .Must(BeAValidUrl).WithMessage("Invalid URL format");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
