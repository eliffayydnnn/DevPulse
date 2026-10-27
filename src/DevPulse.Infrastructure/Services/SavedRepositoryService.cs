using DevPulse.Application.Contracts;
using DevPulse.Domain.Entities;
using DevPulse.Domain.Exceptions;
using DevPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services;

public class SavedRepositoryService : ISavedRepositoryService
{
    private readonly DevPulseDbContext _dbContext;
    private readonly IGitHubService _gitHubService;

    public SavedRepositoryService(DevPulseDbContext dbContext, IGitHubService gitHubService)
    {
        _dbContext = dbContext;
        _gitHubService = gitHubService;
    }

    public async Task<IReadOnlyList<SavedRepository>> GetSavedRepositoriesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SavedRepositories
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.SavedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<SavedRepository> SaveRepositoryAsync(Guid userId, string owner, string name, string? notes = null, CancellationToken cancellationToken = default)
    {
        // Check if user already saved this repo
        var existing = await _dbContext.SavedRepositories
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Owner.ToLower() == owner.ToLower() && r.Name.ToLower() == name.ToLower(), cancellationToken);

        if (existing != null)
        {
            if (notes != null)
            {
                existing.Notes = notes;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            return existing;
        }

        // Fetch latest repository details from GitHub to populate snapshot metadata
        var repo = await _gitHubService.GetRepositoryAsync(owner, name, cancellationToken);

        var saved = new SavedRepository
        {
            UserId = userId,
            Owner = repo.Owner,
            Name = repo.Name,
            Description = repo.Description,
            PrimaryLanguage = repo.PrimaryLanguage,
            StarCountAtSave = repo.Stars,
            ForkCountAtSave = repo.Forks,
            Notes = notes,
            SavedAtUtc = DateTime.UtcNow
        };

        _dbContext.SavedRepositories.Add(saved);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return saved;
    }

    public async Task DeleteSavedRepositoryAsync(Guid userId, Guid savedRepoId, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.SavedRepositories
            .FirstOrDefaultAsync(r => r.Id == savedRepoId && r.UserId == userId, cancellationToken);

        if (item == null)
        {
            throw new NotFoundException("SavedRepository", savedRepoId);
        }

        _dbContext.SavedRepositories.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
