namespace DevPulse.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public string ResourceName { get; }
    public object ResourceKey { get; }

    public NotFoundException(string resourceName, object resourceKey)
        : base($"Resource '{resourceName}' with identifier '{resourceKey}' was not found.")
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }
}
