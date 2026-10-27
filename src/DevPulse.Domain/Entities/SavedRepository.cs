namespace DevPulse.Domain.Entities;

public class SavedRepository
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PrimaryLanguage { get; set; }
    public int StarCountAtSave { get; set; }
    public int ForkCountAtSave { get; set; }
    public string? Notes { get; set; }
    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
}
