using Microsoft.Extensions.Options;

namespace SwiftLift.Infrastructure.Options.Builder;

internal static partial class OptionsBuilderFluentValidationExtensions
{
    public static OptionsBuilder<TOptions> ValidateConfigurationSection<TOptions>(this OptionsBuilder<TOptions> builder, string sectionPath)
       where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(sectionPath);

        builder.Services
            .AddSingleton<IValidateOptions<TOptions>>(
                sp => ActivatorUtilities.CreateInstance<ConfigurationSectionValidateOptions<TOptions>>(
                    sp, builder.Name, sectionPath));

        return builder;
    }

    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .AddSingleton<IValidateOptions<TOptions>>(
                sp => ActivatorUtilities.CreateInstance<FluentValidationValidateOptions<TOptions>>(
                    sp, builder.Name));

        return builder;
    }
}
