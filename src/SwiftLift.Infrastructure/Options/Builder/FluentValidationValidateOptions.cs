using System.Globalization;
using Microsoft.Extensions.Options;

namespace SwiftLift.Infrastructure.Options.Validation;

internal sealed class FluentValidationValidateOptions<TOptions>(
    IValidator<TOptions> _validator,
    string? _name)
        : IValidateOptions<TOptions>
            where TOptions : class
{
    private static readonly Type s_optionsType = typeof(TOptions);

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (_name != null && _name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        Guard.Against.Null(options);

        var results = _validator.ValidateAsync(options)
            .GetAwaiter().GetResult();

        if (results.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var optionsDescription =
            string.IsNullOrWhiteSpace(_name)
            ? s_optionsType.FullName
            : $"{s_optionsType.FullName}(Named: {_name})";

        var sb = new StringBuilder($"Fluent validation failed for {optionsDescription}");

        results.Errors
            .ForEach(result => sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"Property '{result.PropertyName}' with the error: '{result.ErrorMessage}'."));

        return ValidateOptionsResult.Fail(sb.ToString());
    }
}
