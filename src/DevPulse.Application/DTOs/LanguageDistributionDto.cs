namespace DevPulse.Application.DTOs;

public class LanguageDistributionDto
{
    public string Language { get; set; } = string.Empty;
    public long Bytes { get; set; }
    public double Percentage { get; set; }
    public string? ColorHex { get; set; }
}
