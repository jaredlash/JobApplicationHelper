using JobApplicationHelper.Domain.Models;

namespace JobApplicationHelper.Application.Repositories;

public interface IExperienceRepository
{
    Task<IReadOnlyList<Experience>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Experience?> GetAsync(ExperienceId id, CancellationToken cancellationToken = default);

    Task<Experience?> GetByLegacyIdAsync(string legacyId, CancellationToken cancellationToken = default);

    Task AddOrReplaceAsync(Experience experience, string? legacyId = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(ExperienceId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetContextSuggestions(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetSkillSuggestions(CancellationToken cancellationToken = default);
}