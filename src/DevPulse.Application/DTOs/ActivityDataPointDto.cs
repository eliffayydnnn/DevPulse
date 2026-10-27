namespace DevPulse.Application.DTOs;

public class ActivityDataPointDto
{
    public string PeriodLabel { get; set; } = string.Empty; // e.g., "2026-W42" or "Oct 2026"
    public DateTime PeriodStart { get; set; }
    public int Commits { get; set; }
    public int PullRequests { get; set; }
    public int Issues { get; set; }
}
