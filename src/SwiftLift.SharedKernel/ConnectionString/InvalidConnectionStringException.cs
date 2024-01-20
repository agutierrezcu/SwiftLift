namespace SwiftLift.SharedKernel.ConnectionString;

public sealed class InvalidConnectionStringException : ApplicationException
{
    public InvalidConnectionStringException(string resourceName, string message)
        : base(message)
    {
        ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
    }

    public InvalidConnectionStringException(string resourceName, string message, Exception innerException)
        : base(message, innerException)
    {
        ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
    }

    public string ResourceName { get; }
}
