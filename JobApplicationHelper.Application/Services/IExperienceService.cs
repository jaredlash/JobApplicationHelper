using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Services;

public interface IExperienceService
{
    Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Experience?> GetAsync(ExperienceId id, CancellationToken cancellationToken = default);

    Task AddOrReplaceAsync(Experience experience, CancellationToken cancellationToken = default);

    Task DeleteAsync(ExperienceId id, CancellationToken cancellationToken = default);
}