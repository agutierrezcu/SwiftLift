namespace SwiftLift.Infrastructure.Configuration;

public static class ConfigurationCollectionExtensions
{
    public static string GetRequired(this IConfiguration configuration,
        string configKey)
    {
        Guard.Against.Null(configuration);
        Guard.Against.NullOrWhiteSpace(configKey);

        var configValue = configuration[configKey];

        if (string.IsNullOrWhiteSpace(configValue))
        {
            throw new InvalidOperationException($"Configuration value is not defined or value is not set for {configKey} key");
        }

        return configValue;
    }
}
