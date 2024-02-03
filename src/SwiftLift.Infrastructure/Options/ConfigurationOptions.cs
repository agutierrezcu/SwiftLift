using static Microsoft.Extensions.Options.Options;

namespace SwiftLift.Infrastructure.Options;

public sealed class ConfigurationOptions
{
    private string _name = DefaultName;

    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value) ? DefaultName : value.Trim();
    }

    public bool RegisterAsSingleton { get; set; } = true;

    public bool ErrorOnUnknownConfiguration { get; set; }
}
