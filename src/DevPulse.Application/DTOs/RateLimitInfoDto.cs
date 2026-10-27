namespace DevPulse.Application.DTOs;

public class RateLimitInfoDto
{
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public DateTime? ResetTimeUtc { get; set; }
    public int Used => Math.Max(0, Limit - Remaining);
    public double PercentageRemaining => Limit > 0 ? Math.Round((double)Remaining / Limit * 100, 1) : 0;
}
