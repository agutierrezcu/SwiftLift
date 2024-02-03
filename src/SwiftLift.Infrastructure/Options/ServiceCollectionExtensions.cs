using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SwiftLift.Infrastructure.Options.Validation;

namespace SwiftLift.Infrastructure.Options;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureOptions<TOptions, TOptionsValidator>(
        this IServiceCollection services,
        string sectionPath,
        Action<ConfigurationOptions>? configureAction = null)
            where TOptions : class
            where TOptionsValidator : class, IValidator<TOptions>
    {
        Guard.Against.Null(services);
        Guard.Against.NullOrWhiteSpace(sectionPath);

        ConfigurationOptions options = new();
        configureAction?.Invoke(options);

        services.TryAddSingleton<IValidator<TOptions>, TOptionsValidator>();

        var optionsBuilder = services
            .AddOptions<TOptions>(options.Name)
            .BindConfiguration(sectionPath,
                opts =>
                {
                    opts.BindNonPublicProperties = true;
                    opts.ErrorOnUnknownConfiguration = options.ErrorOnUnknownConfiguration;
                })
            .ValidateConfigurationSection(sectionPath)
            .ValidateDataAnnotations()
            .ValidateFluentValidation()
            .ValidateOnStart();

        if (options.RegisterAsSingleton)
        {
            services.AddSingleton(
                sp => sp.GetRequiredService<IOptions<TOptions>>().Value);
        }

        return services;
    }
}
