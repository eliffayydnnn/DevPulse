using DevPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Persistence;

public class DevPulseDbContext : DbContext
{
    public DevPulseDbContext(DbContextOptions<DevPulseDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SavedRepository> SavedRepositories => Set<SavedRepository>();
    public DbSet<RepositorySnapshot> RepositorySnapshots => Set<RepositorySnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);

            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
        });

        // SavedRepository configuration
        modelBuilder.Entity<SavedRepository>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Owner).IsRequired().HasMaxLength(150);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(150);
            builder.Property(r => r.Description).HasMaxLength(1000);
            builder.Property(r => r.PrimaryLanguage).HasMaxLength(100);
            builder.Property(r => r.Notes).HasMaxLength(2000);

            builder.HasIndex(r => new { r.UserId, r.Owner, r.Name }).IsUnique();
            builder.HasIndex(r => new { r.Owner, r.Name });

            builder.HasOne(r => r.User)
                   .WithMany(u => u.SavedRepositories)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // RepositorySnapshot configuration
        modelBuilder.Entity<RepositorySnapshot>(builder =>
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Owner).IsRequired().HasMaxLength(150);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
            builder.Property(s => s.PrimaryLanguage).HasMaxLength(100);
            builder.Property(s => s.SummaryMetricsJson).IsRequired();

            builder.HasIndex(s => new { s.Owner, s.Name });
            builder.HasIndex(s => s.AnalyzedAtUtc);
        });
    }
}
