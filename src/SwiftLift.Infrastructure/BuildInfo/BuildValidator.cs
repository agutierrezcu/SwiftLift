namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildValidator : AbstractValidator<Build>
{
    public BuildValidator()
    {
        RuleFor(build => build.Id).NotEmpty();

        RuleFor(build => build.Number).NotEmpty();

        RuleFor(build => build.Branch).NotEmpty();

        RuleFor(build => build.Commit).NotEmpty();

        RuleFor(build => build.Url).NotEmpty()
            .Must(BeAValidUrl).WithMessage("Invalid URL format.");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
