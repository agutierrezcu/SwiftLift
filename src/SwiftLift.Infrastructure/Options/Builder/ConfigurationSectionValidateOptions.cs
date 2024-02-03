using Microsoft.Extensions.Options;

namespace SwiftLift.Infrastructure.Options.Validation;

internal sealed class ConfigurationSectionValidateOptions<TOptions>(
    IConfiguration _configuration,
    string? _name,
    string sectionPath)
        : IValidateOptions<TOptions>
            where TOptions : class
{
    private readonly string _sectionPath = sectionPath ??
        throw new ArgumentNullException(nameof(sectionPath));

    public ValidateOptionsResult Validate(string? name, TOptions _)
    {
        if (name != null && name != _name)
        {
            return ValidateOptionsResult.Skip;
        }

        var configurationSection = _configuration.GetSection(_sectionPath);

        if (configurationSection.Exists())
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail($"Configuration section with key '{_sectionPath}' aimed to bind '{typeof(TOptions).FullName}' settings does not exist .");
    }
}
