namespace SwiftLift.Infrastructure.ConnectionString;

public sealed class InvalidConnectionStringException : ApplicationException
{
    public InvalidConnectionStringException(string resourceName, string message)
        : base(message)
    {
        ResourceName = Guard.Against.NullOrWhiteSpace(resourceName);
    }

    public InvalidConnectionStringException(string resourceName, string message, Exception innerException)
        : base(message, innerException)
    {
        ResourceName = Guard.Against.NullOrWhiteSpace(resourceName);
    }

    public string ResourceName { get; }
}
