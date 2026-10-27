using DevPulse.Domain.Entities;

namespace DevPulse.Application.Contracts;

public interface ISavedRepositoryService
{
    Task<IReadOnlyList<SavedRepository>> GetSavedRepositoriesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SavedRepository> SaveRepositoryAsync(Guid userId, string owner, string name, string? notes = null, CancellationToken cancellationToken = default);
    Task DeleteSavedRepositoryAsync(Guid userId, Guid savedRepoId, CancellationToken cancellationToken = default);
}
