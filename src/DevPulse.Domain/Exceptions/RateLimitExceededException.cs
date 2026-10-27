namespace DevPulse.Domain.Exceptions;

public class RateLimitExceededException : DomainException
{
    public DateTime? ResetTimeUtc { get; }
    public int? RemainingLimit { get; }

    public RateLimitExceededException(string message, DateTime? resetTimeUtc = null, int? remainingLimit = null)
        : base(message)
    {
        ResetTimeUtc = resetTimeUtc;
        RemainingLimit = remainingLimit;
    }
}
