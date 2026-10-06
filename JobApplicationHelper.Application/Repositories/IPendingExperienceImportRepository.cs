using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Repositories;

public interface IPendingExperienceImportRepository
{
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<ExperienceId?> GetNextPendingImportAsync(CancellationToken cancellationToken = default);

    Task<Experience?> GetAsync(ExperienceId experienceId, CancellationToken cancellationToken = default);

    Task AddOrReplaceAsync(Experience experience, CancellationToken cancellationToken = default);

    Task DeleteAsync(ExperienceId experienceId, CancellationToken cancellationToken = default);
}